import React from "react";
import { CommandButton } from "./components/commandButton";
import styled from 'styled-components';
import { Spinner } from "./components/AnimatedIcons";

export const LeaderBoard = styled.div`
  background-color: rgba(0, 0, 0, 0.8);
  position: absolute;
  top: 0px;
  height: 80%;
  width: 100%;
  overflow: auto;
`;

const PostMyScore = styled.div`
  background-color: rgba(0, 0, 0, 0.8);
  position: absolute;
  bottom: 0px;
  height: 20%;
  width: 100%;
`;

const PostMyScoreButton = styled(CommandButton)`
  position: relative;
  opacity: 1.0;
  margin-top: 7px;
`;

const UserNameCell = styled.td`
  text-align: left;
`;

export function ScoreBoard({ game, postableScore, onPostScore: postScore, isLoading, username }) {
  const allowScorePost = game.paused && Boolean(postableScore);

  const scoreBoard = game.paused && (game.scoreBoard || isLoading) &&
    <LeaderBoard style={{ height: allowScorePost ? "80%" : "100%" }}>
      <table className="table">
        <thead>
          {Boolean(game.scoreBoard?.length) && <tr>
            <th colSpan="2" style={{ textAlign: "center" }}>
              Top {game.scoreBoard?.length} scores
            </th>
          </tr>}
        </thead>
        <tbody>
          {isLoading ? (
            <tr>
              <td>
                <b><Spinner /> Загрузка...</b>
              </td>
            </tr>
          ) : (
            game.scoreBoard.map(userScore => (
              <tr key={userScore.username}>
                <UserNameCell>{userScore.username}</UserNameCell>
                <td>{userScore.score}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </LeaderBoard>;

  return <>
    {scoreBoard}
    {allowScorePost && (
      <PostMyScore>
        Опубликовать ваш результат?
        <PostMyScoreButton
          className="btn btn-primary"
          runningText={Boolean(username) ? <><Spinner /> Публикация вашего результата...</> : undefined}
          onClick={postScore}>
          Post My Score ({postableScore})
        </PostMyScoreButton>
      </PostMyScore>
    )}
  </>;
}
