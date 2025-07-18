import { useLoadingState, useMountedOnlyState } from "leaf-validator";
import React, { useRef } from "react";
import { useLifeCycle } from "./hooks/useLifeCycle";
import { QuietRest } from "./services/rest";
import { Link } from "react-router-dom";
import { Pager } from "./components/Pager";
import styled from "styled-components";
import { MultiplayerLinks } from "./MultiplayerLinks";
import { Spinner } from "./components/AnimatedIcons";

const BoldRed = styled.strong`
  color: red;
`;

const BoldGreen = styled.strong`
  color: green;
`;

const Statuses = [
  <BoldRed>Игра идет</BoldRed>,
  <BoldGreen>Ожидаем игроков</BoldGreen>,
];
const ItemsPerPage = 5;

export const GameRooms = () => {
  const [gameRooms, setGameRooms] = useMountedOnlyState();
  const [isLoading, showLoadingWhile] = useLoadingState();
  const pageRef = useRef(1);
  const timerRef = useRef();

  const refresh = async () => {
    try {
      const url = `/api/rooms?start=${
        (pageRef.current - 1) * ItemsPerPage
      }&count=${ItemsPerPage}`;
      await QuietRest.get(url).then(setGameRooms);
    } catch (err) {
      console.warn(err);
    } finally {
      timerRef.current = setTimeout(refresh, 10000);
    }
  };

  const requestPage = (page) => {
    clearTimeout(timerRef.current);
    pageRef.current = page;
    return refresh();
  };

  useLifeCycle({
    onMount: () => showLoadingWhile(refresh()),
    onUnMount: () => clearTimeout(timerRef.current),
  });

  return (
    <div className="card mt-3 mb-3">
      <div className="card-body">
        <h5 className="card-title">Доступные комнаты</h5>
        <div className="card-text">
          <table className="table">
            <thead>
              <tr>
                <th></th>
                <th>Статус</th>
                <th>Игроки</th>
                <th>Код</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td colSpan={4} className="centered">
                    <strong>
                      <Spinner /> Загрузка...
                    </strong>
                  </td>
                </tr>
              ) : gameRooms?.items?.length ? (
                gameRooms?.items?.map((room) => (
                  <tr key={room.organizerId}>
                    <td>
                      <Link to={`/${room.organizerId}`}>Присоединиться</Link>
                    </td>
                    <td>{Statuses[room.status]}</td>
                    <td>
                      {Object.values(room.players)
                        .map((player) => player.username ?? "[Неизвестный]")
                        .join(", ")}
                    </td>
                    <td>{room.organizerId}</td>
                  </tr>
                ))
              ) : (
                <tr>
                  <td colSpan={4}>
                    <strong>В данный момент отсутствуют активные лобби</strong>
                  </td>
                </tr>
              )}
            </tbody>
          </table>
          {gameRooms?.items?.length > 0 && (
            <Pager
              page={Math.ceil(gameRooms?.start / ItemsPerPage) + 1}
              numPages={Math.ceil(gameRooms?.total / ItemsPerPage)}
              onPageChange={requestPage}
            />
          )}
          <MultiplayerLinks />
        </div>
      </div>
    </div>
  );
};
