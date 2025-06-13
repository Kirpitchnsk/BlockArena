import React, { useCallback, useEffect, useRef, useState } from "react";
import { flushSync } from "react-dom";
import { TetrisBoard } from "./TetrisBoard";
import { tetrisBoardFrom } from "../domain/serialization";
import { move, rotate } from "../domain/motion";
import { iterate, iterateUntilInactive } from "../domain/iteration";
import { keys } from "../core/constants";
import { MobileControls } from "./MobileControls";

export const shapes = [
  [
    [true, true],
    [true, true],
  ],
  [[true], [true], [true], [true]],
  [
    [true, false],
    [true, false],
    [true, true],
  ],
  [
    [false, true],
    [false, true],
    [true, true],
  ],
  [
    [false, true],
    [true, true],
    [true, false],
  ],
  [
    [true, false],
    [true, true],
    [false, true],
  ],
  [
    [false, true, false],
    [true, true, true],
  ],
];

export const emptyBoard = tetrisBoardFrom(`
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
    ----------
`);

export const TetrisGame = ({
  game: gameState,
  onChange,
  shapeProvider,
  onPause,
  onLinesCleared,
}) => {
  const { board, mobile, oldScore, paused, score } = gameState;
  const [explodingRows, setExplodingRows] = useState([]);

  const instance = useRef({
    onChange,
    shapeProvider,
    onLinesCleared,
    gameState,
  });

  instance.current = {
    onChange,
    shapeProvider,
    onLinesCleared,
    gameState,
  };

  useEffect(() => {
    if (explodingRows.length > 0) {
      const timer = setTimeout(() => {
        setExplodingRows([]);
      }, 500); // ⏱️ очистка взрывающих строк через 0.5 сек

      return () => clearTimeout(timer);
    }
  }, [explodingRows]);

  const cycle = useCallback(() => {
    if (!instance.current.gameState.paused) {
      flushSync(() => {
        instance.current.onChange((game) => {
          const { board, score } = game;
          const iteratedGame = iterate({
            board,
            score,
            shapeProvider: instance.current.shapeProvider,
          });

          if (
            iteratedGame.explodingRows &&
            iteratedGame.explodingRows.length > 0
          ) {
            setExplodingRows(iteratedGame.explodingRows);

            // ⚠️ безопасный вызов setState (отложенный)
            setTimeout(() => {
              instance.current.onLinesCleared?.(
                iteratedGame.explodingRows.length
              );
            }, 0);
          }

          return iteratedGame.isOver
            ? {
                ...game,
                board: emptyBoard,
                score: 0,
                oldScore: game.score,
              }
            : { ...game, ...iteratedGame };
        });
      });
    }
  }, []);

  const keyPress = useCallback((event) => {
    const { keyCode } = event;

    const processKeyCommand = () => {
      const { board } = instance.current.gameState;

      const moves = {
        [keys.left]: () => move({ board, to: { x: -1 } }),
        [keys.right]: () => move({ board, to: { x: 1 } }),
        [keys.down]: () => move({ board, to: { y: 1 } }),
        [keys.up]: () => rotate({ board }),
        [keys.space]: () => iterateUntilInactive({ board }),
      };

      const selectedMove = moves[keyCode];
      if (selectedMove) {
        event.preventDefault?.();
        flushSync(() => {
          instance.current.onChange((game) => ({
            ...game,
            board: selectedMove(),
          }));
        });
      }
    };

    if (!instance.current.gameState.paused) {
      processKeyCommand();
    }
  }, []);

  useEffect(() => {
    window.addEventListener("keydown", keyPress, false);
    window.addEventListener("iterate-game", cycle, false);

    return () => {
      window.removeEventListener("keydown", keyPress, false);
      window.removeEventListener("iterate-game", cycle, false);
    };
  }, [cycle, keyPress]);

  return (
    <div>
      {!gameState.paused && gameState.mobile && (
        <MobileControls
          onPause={onPause}
          onClick={(keyCode) => keyPress({ keyCode })}
        />
      )}
      <TetrisBoard
        board={gameState.board}
        explodingRows={explodingRows}
      />
    </div>
  );
};