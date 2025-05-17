using BlockArena.Common.Exceptions;
using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockArena.Common.Ratings
{
    public class RatingUpdater(IRatingStorage scoreBoardStorage, Func<Task<Rating>> getLeaderBoard) : IRatingUpdater
    {
        private readonly Func<Task<Rating>> getLeaderBoard = getLeaderBoard;
        private readonly IRatingStorage scoreBoardStorage = scoreBoardStorage;

        public async Task Add(UserScore userScore)
        {
            var trimmedUserScore = new UserScore { Username = userScore.Username.Trim(), Score = userScore.Score };

            if (trimmedUserScore.Username.Length > Constants.MaxUsernameChars)
                throw new ValidationException($"Username length must not be over {Constants.MaxUsernameChars}.");

            var leaderBoard = await getLeaderBoard();

            var firstRepeat = (leaderBoard.UserScores ?? new List<UserScore>())
                .FirstOrDefault(currentUserScore =>
                    trimmedUserScore.Username.ToLower() == currentUserScore.Username.ToLower()
                    && userScore.Score <= currentUserScore.Score);

            if (firstRepeat != null)
            {
                throw new ValidationException($"{firstRepeat.Username} already has a score equal to or greater than {userScore.Score}.");
            }

            await scoreBoardStorage.Add(trimmedUserScore);
        }
    }
}