import React, { useState } from "react";
import { TetrisGame, emptyBoard } from "./components/TetrisGame";
import { StringInput as StringPrompt, usePrompt } from "./components/Prompt";
import { leaderBoardService } from "./services";
import "./App.css";
import { ScoreBoard } from "./Rating";
import { GameControls } from "./GameControls";
import {
  createManagedContext,
  useLoadingState,
  useMountedOnlyState,
} from "leaf-validator";
import { GameMetaFrame } from "./components/GameMetaFrame";
import { useSessionStorageState } from "./hooks/useSessionStorageState";
import { useIsMobile } from "./hooks/useIsMobile";
import { Spinner } from "./components/AnimatedIcons";
import styled from "styled-components";
import { TetrisBoard } from "./components/TetrisBoard";

export const initialGameState = {
  board: emptyBoard,
  isOver: false,
  oldScore: 0,
  paused: true,
  score: 0,
};

export const [SinglePlayerGameContextProvider, useLocalPlayerGameContext] =
  createManagedContext(({ shapeProvider }) => {
    const [game, setGame] = useMountedOnlyState(initialGameState);
    const [nextShape, setNextShape] = useState(shapeProvider());
    const isMobile = useIsMobile();
    const [username, setUsername] = useSessionStorageState("username");
    const { dialogProps, prompt } = usePrompt();

    const [isLoadingScoreBoard, showLoadingScoreBoardWhile] = useLoadingState();

    const postableScore = game.score || game.oldScore;
    const allowScorePost = game.paused && Boolean(postableScore);

    const postScore = async () => {
      const hasUserName = Boolean(username?.trim().length);

      const sendCurrentScoreFor = (name) =>
        leaderBoardService
          .postScore({
            username: name,
            score: postableScore,
          })
          .then(reloadScoreBoard)
          .then((scoreBoard) => {
            const scoreIsOnBoard = scoreBoard.find(
              ({ username }) => username === name
            );
            !scoreIsOnBoard &&
              window.dispatchEvent(
                new CustomEvent("user-error", {
                  detail: `Ваш результат был зарегистрирован, но не попал в топ ${scoreBoard.length}.`,
                })
              );
          });

      const promptUserNameAndSendScore = () =>
        prompt((exitModal) => (
          <StringPrompt
            filter={(value) => (value ?? "").trim()}
            onSubmitString={(name) =>
              Boolean(name?.length)
                ? sendCurrentScoreFor(name)
                    .then(() => setUsername(name))
                    .then(exitModal, exitModal)
                : exitModal()
            }
            submittingText={
              <>
                <Spinner /> Отправка вашего результата...
              </>
            }
          >
            Наишите ваш игровой ник
          </StringPrompt>
        ));

      await (hasUserName
        ? sendCurrentScoreFor(username)
        : promptUserNameAndSendScore());
    };

    const reloadScoreBoard = async () => {
      const scoreBoard = await leaderBoardService.get();
      setGame((game) => ({ ...game, scoreBoard }));
      return scoreBoard;
    };

    const pause = async ({ showScoreBoard }) => {
      const paused = !game.paused;
      setGame({
        ...game,
        paused,
        scoreBoard: paused ? game.scoreBoard : undefined,
      });
      paused &&
        showScoreBoard &&
        (await showLoadingScoreBoardWhile(reloadScoreBoard()));
    };

    const nextShapeProvider = () => {
      const shape = nextShape;
      setNextShape(shapeProvider());
      return shape;
    };

    return {
      game: { ...game, mobile: isMobile },
      setGame,
      username,
      setUsername,
      dialogProps,
      prompt,
      isLoadingScoreBoard,
      showLoadingScoreBoardWhile,
      postableScore,
      allowScorePost,
      postScore,
      reloadScoreBoard,
      pause,
      nextShape,
      nextShapeProvider,
    };
  });

const NextShapeContainer = styled.div`
  position: fixed;
  top: 70px;
  left: 240px;
  opacity: 0.5;
`;

export const LocalPlayerGame = ({
  shapeProvider,
  children: otherPlayers,
  header,
  additionalControls,
  isOnlyPlayer,
  onLinesCleared, 
  ...otherMetaProps 
}) => {
  const {
    game,
    setGame,
    username,
    pause,
    allowScorePost,
    isLoadingScoreBoard,
    postableScore,
    postScore,
    nextShape,
    nextShapeProvider,
  } = useLocalPlayerGameContext();

  return (
    <>
      {!game.paused && (
        <NextShapeContainer>
          <TetrisBoard board={nextShape.map(row =>
            row.map(cell => ({ type: cell ? "active" : "empty" }))
          )} noBackground />
        </NextShapeContainer>
      )}
      <GameMetaFrame
        {...otherMetaProps} 
        header={
          <>
            {header}
            <p>
              {(isOnlyPlayer || !game.paused) &&
                `Счёт: ${game.score}` +
                  (game.oldScore ? ` (Предыдущий рекорд: ${game.oldScore})` : "")}
            </p>
          </>
        }
        game={
          <TetrisGame
            game={game}
            onChange={setGame}
            shapeProvider={nextShapeProvider}
            onPause={
              !otherPlayers && (() => pause({ showScoreBoard: !otherPlayers }))
            }
            onLinesCleared={onLinesCleared}  
          />
        }
        scoreBoard={
          game.paused &&
          (otherPlayers || (
            <ScoreBoard
              allowScorePost={allowScorePost}
              game={game}
              username={username}
              isLoading={isLoadingScoreBoard}
              onPostScore={postScore}
              postableScore={postableScore}
            />
          ))
        }
        controls={
          <>
            <GameControls
              game={game}
              onPause={
                !otherPlayers &&
                (() => pause({ showScoreBoard: !otherPlayers }))
              }
            />
            {additionalControls}
          </>
        }
      />
    </>
  );
};

export default LocalPlayerGame;
