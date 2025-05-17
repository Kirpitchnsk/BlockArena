using BlockArena.Common.Exceptions;
using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockArena.Common.Ratings
{
    public class RatingUpdater(IRatingStorage ratingStorage, Func<Task<Rating>> getRating) : IRatingUpdater
    {
        private readonly Func<Task<Rating>> getLeaderBoard = getRating;
        private readonly IRatingStorage ratingStorage = ratingStorage;

        public async Task Add(UserScore userScore)
        {
            var trimUserScore = new UserScore { Username = userScore.Username.Trim(), Score = userScore.Score };

            if (trimUserScore.Username.Length > Constants.MaxUsernameChars)
            {
                throw new ValidationException($"Имя не должно быть больше {Constants.MaxUsernameChars}.");
            }

            var rating = await getLeaderBoard();

            var firstRepeat = (rating.UserScores ?? new List<UserScore>())
                .FirstOrDefault(currentUserScore =>
                    trimUserScore.Username.ToLower() == currentUserScore.Username.ToLower()
                    && userScore.Score <= currentUserScore.Score);

            if (firstRepeat != null)
            {
                throw new ValidationException($"{firstRepeat.Username} уже имеет рекорд {userScore.Score}.");
            }

            await ratingStorage.Add(trimUserScore);
        }
    }
}