import React from "react";
import { activeColumnRangeFrom } from "../domain/board";
import { empty } from "../core/constants";
import styled, { keyframes } from "styled-components";
import { explosionAnimation } from "./AnimatedIcons";

const SQUARE_SIZE = 29.3;

const COLORS = [
"#F56AA3", 
"#6AADF5", 
"#6AF5AA", 
"#D778DD",
"#F5B16A",
"#F5E878",
"#7891F5" 
];

const Square = styled.div`
  width: ${SQUARE_SIZE}px;
  height: ${SQUARE_SIZE}px;
  box-sizing: border-box;
  border: 1px solid #ffffff; /* белая обводка */
`;

const ExplodingSquare = styled(Square)`
  animation: ${explosionAnimation} 0.5s ease-out forwards;
`;

function getColor(x, y) {
  return COLORS[(x + y) % COLORS.length];
}

export function TetrisBoard({
  board,
  explodingRows = [],
  noBackground = false,
}) {
  const activeColumnRange = activeColumnRangeFrom({ board });

  const renderCell = ({ square, x, y }) => {
    if (explodingRows.includes(y)) {
      return (
        <ExplodingSquare
          data-testid="space"
          title="#"
          style={{ backgroundColor: "#333" }}
        />
      );
    }

    const isEmpty = square === empty;
    const inActiveZone =
      isEmpty && x >= activeColumnRange.x1 && x <= activeColumnRange.x2;

    if (isEmpty) {
      return (
        <Square
          data-testid="space"
          title="-"
          style={{ backgroundColor: inActiveZone ? "#222" : "#111" }}
        />
      );
    } else {
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
