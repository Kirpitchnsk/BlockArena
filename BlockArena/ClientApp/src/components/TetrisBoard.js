// src/components/TetrisBoard.js
import React from "react";
import { activeColumnRangeFrom } from "../domain/board";
import { empty } from "../core/constants";
import styled, { keyframes } from "styled-components";
import { explosionAnimation } from "./AnimatedIcons";

// Ширина/высота клеток
const SQUARE_SIZE = 29.3;

// Палитра цветов для тетромино
const COLORS = [
"#F56AA3", // I (было #FF0D72)
"#6AADF5", // J (было #0DC2FF)
"#6AF5AA", // L (было #0DFF72)
"#D778DD", // O (было #F538FF)
"#F5B16A", // S (было #FF8E0D)
"#F5E878", // T (было #FFE138)
"#7891F5"  // Z (было #3877FF)
];

// Базовый компонент «квадратик»
const Square = styled.div`
  width: ${SQUARE_SIZE}px;
  height: ${SQUARE_SIZE}px;
  box-sizing: border-box;
  border: 1px solid #ffffff; /* белая обводка */
`;

// Анимированный квадрат при взрыве
const ExplodingSquare = styled(Square)`
  animation: ${explosionAnimation} 0.5s ease-out forwards;
`;

// Вспомогательная функция: цвет активной клетки в зависимости от её координат
function getColor(x, y) {
  // простой детерминированный выбор цвета: сумма индексов по модулю палитры
  return COLORS[(x + y) % COLORS.length];
}

export function TetrisBoard({
  board,
  explodingRows = [],
  noBackground = false,
}) {
  const activeColumnRange = activeColumnRangeFrom({ board });

  const renderCell = ({ square, x, y }) => {
    // если строка «взрывается» — показываем анимированную клетку
    if (explodingRows.includes(y)) {
      return (
        <ExplodingSquare
          data-testid="space"
          title="#"
          style={{ backgroundColor: "#333" }}
        />
      );
    }

    // пустая или занятая область
    const isEmpty = square === empty;
    const inActiveZone =
      isEmpty && x >= activeColumnRange.x1 && x <= activeColumnRange.x2;

    if (isEmpty) {
      // пустая зона внутри активной области — чуть светлее
      return (
        <Square
          data-testid="space"
          title="-"
          style={{ backgroundColor: inActiveZone ? "#222" : "#111" }}
        />
      );
    } else {
      // занятая (active) — разноцветно
      return (
        <Square
          data-testid="space"
          title="*"
          style={{ backgroundColor: getColor(x, y) }}
        />
      );
    }
  };

  return (
    <table style={noBackground ? undefined : { backgroundColor: "#000" }}>
      <tbody>
        {board.map((row, y) => (
          <tr key={y} data-testid="row">
            {row.map((square, x) => (
              <td key={`${x},${y}`} style={{ padding: 0 }}>
                {renderCell({ square, x, y })}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
