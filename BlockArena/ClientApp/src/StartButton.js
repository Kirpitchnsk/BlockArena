import React from "react";
import { CommandButton } from "./components/commandButton";
import { useOrganizerId } from "./hooks/useOrganizerId";
import { useMultiplayerContext } from "./MultiplayerContext";
import { Spinner } from "./components/AnimatedIcons";

export const BigStartButton = () => {
  const organizerUserId = useOrganizerId();
  const {
    gameHub,
    userId: currentUserId,
    canGuestStartGame,
  } = useMultiplayerContext();
  const isOrganizer = organizerUserId === currentUserId;
  const isStartable = isOrganizer || canGuestStartGame;

  const startGame = () => gameHub.invoke.start({ groupId: organizerUserId });

  return (
    <CommandButton
      style={{ width: "100%" }}
      disabled={!isStartable}
      onClick={startGame}
      runningText={<><Spinner /> Старт...</>}
      className="btn btn-success btn-lg mb-3"
    >
      Начать игру
    </CommandButton>
  );
};
