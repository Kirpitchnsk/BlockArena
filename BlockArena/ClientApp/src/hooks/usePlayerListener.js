import { useEffect, useRef, useState } from "react"; 
import { update, process } from "../domain/players";
import { useMultiplayerContext } from "../MultiplayerContext";
import {
  initialGameState,
  useLocalPlayerGameContext,
} from "../LocalPlayerGame";
import { stringFrom } from "../domain/serialization";
import { useOrganizerId } from "./useOrganizerId";

const MaxChatLines = 10;
const audio = new Audio('/chat-notification.mp3');

function scrollToTop() {
  window.scrollTo({ top: 0, behavior: 'smooth' });
}

export const usePlayerListener = () => {
  const organizerUserId = useOrganizerId();
  const {
    gameHub,
    isConnected,
    userId: currentUserId,
    timeProvider,
    setGameEndTime,
    setOrganizerConnectionStatus,
    setOtherPlayers,
    setGameResults,
    selectedDuration,
    setCanGuestStartGame,
    chatLines,
    setChatLines,
    soundEnabled
  } = useMultiplayerContext();
  const { game, setGame, username } = useLocalPlayerGameContext();
  const isOrganizer = organizerUserId === currentUserId;

  const externalsRef = useRef({
    gameHub,
    isOrganizer,
    username,
    setOtherPlayers,
    setGameResults,
    setGame,
    game,
    selectedDuration,
    chatLines,
    soundEnabled,
  });

  externalsRef.current = {
    gameHub,
    isOrganizer,
    username,
    setOtherPlayers,
    setGameResults,
    setGame,
    game,
    selectedDuration,
    chatLines,
    soundEnabled,
  };

  useEffect(() => {
    if (!(currentUserId && isConnected)) return;

    gameHub.receive.setHandlers({
      hello: ({ userId, name, isRunning }) => {
        setOtherPlayers((otherPlayers) => ({
          ...otherPlayers,
          [userId]: { name, score: 0, disconnected: false },
        }));

        gameHub.invoke.setChatLines({
          groupId: organizerUserId,
          message: externalsRef.current.chatLines,
        });

        return { status: "active" };
      },

      playersListUpdate: ({ players: updatedPlayersList, isStartable }) => {
        setOtherPlayers((otherPlayers) =>
          update(otherPlayers).with(updatedPlayersList)
        );

        setOrganizerConnectionStatus("connected");
        setCanGuestStartGame(isStartable);

        const isInPlayersList = updatedPlayersList.some(
          ({ userId }) => userId === currentUserId
        );

        if (!isInPlayersList) {
          gameHub.invoke.status({
            groupId: organizerUserId,
            message: {
              userId: currentUserId,
              board: stringFrom(game.board),
              score: game.score,
              name: externalsRef.current.username,
            },
          });
        }
      },

      status: ({ userId, timeLeft, ...otherUpdates }) => {
        if (userId === organizerUserId) {
          setOrganizerConnectionStatus("connected");
        }

        setOtherPlayers((otherPlayers) =>
          process(otherUpdates).on(userId).in(otherPlayers)
        );

        if (!externalsRef.current.isOrganizer && timeLeft) {
          setGameEndTime(timeProvider() + timeLeft);
        }
      },

      start: () => {
        setGame(({ mobile }) => ({
          ...initialGameState,
          mobile,
          paused: false,
        }));

        scrollToTop();
        setGameResults(null);

        if (externalsRef.current.isOrganizer) {
          setGameEndTime(
            timeProvider() + externalsRef.current.selectedDuration
          );
        }
      },

      results: (results) => {
        setGameResults(results);
        setGameEndTime(null);
        setGame((game) => ({ ...game, paused: true }));
      },

      noOrganizer: () => {
        setOrganizerConnectionStatus("disconnected");
      },

      reset: () => {
        setGameResults(null);
        setGameEndTime(null);
        setCanGuestStartGame(true);

        setGame(({ mobile }) => ({
          ...initialGameState,
          mobile,
          paused: true,
        }));

        setOtherPlayers((otherPlayers) =>
          [{}, ...Object.keys(otherPlayers)].reduce(
            (currentPlayers, userId) => ({
              ...currentPlayers,
              [userId]: {
                name: otherPlayers[userId]?.name,
                score: 0,
                disconnected: otherPlayers[userId]?.disconnected,
              },
            }),
            {}
          )
        );
      },

      addToChat: (chatLine) => {
        if (
          chatLine.userId !== currentUserId &&
          externalsRef.current.soundEnabled
        ) {
          audio.play();
        }

        setChatLines((chatLines) =>
          [...chatLines, chatLine].slice(
            Math.max(chatLines.length - (MaxChatLines - 1), 0)
          )
        );
      },

      setChatLines: (chatLines) => setChatLines(chatLines),

      // 🎯 Обработка мусорных строк
      attack: ({ userId, lines }) => {
        console.log("[DEBUG] Пришла атака:", userId, "на", lines);
        if (userId === currentUserId) return;
      
        setGame((game) => {
          const width = game.board[0].length;
      
          // Каждый раз создаём новый garbageRow с уникальными объектами
          const createGarbageRow = () =>
            Array.from({ length: width }, () => ({ type: "inactive" }));
      
          const garbage = Array.from({ length: lines }, () => createGarbageRow());
      
          const newBoard = [...garbage, ...game.board.slice(0, -lines)];
      
          return { ...game, board: newBoard };
        });
      },
    });
  }, [isConnected, currentUserId]);
};
