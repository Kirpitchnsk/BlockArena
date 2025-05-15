using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockArena.Domain.Interfaces;
using BlockArena.Domain.Models;
using BlockArena.Interfaces;

namespace BlockArena.Interactors
{
    public class ScorePipeline(IRatingUpdater ratingUpdater, Func<Task<Rating>> getRating) : IScorePipeline
    {
        private readonly Func<Task<Rating>> getRating = getRating;
        private readonly IRatingUpdater ratingUpdater = ratingUpdater;

        public async Task Add(Models.UserScore userScore)
        {
            await ratingUpdater.Add(new UserScore
            {
                Score = userScore.Score,
                Username = userScore.Username
            });
        }

        public async Task<List<Models.UserScore>> GetUserScores(int count)
        {
            return (await getRating())
                .UserScores
                .OrderByDescending(userScore => userScore.Score)
                .Take(count)
                .Select(userScore => new Models.UserScore
                {
                    Username = userScore.Username,
                    Score = userScore.Score
                })
                .ToList();
        }
    }
}