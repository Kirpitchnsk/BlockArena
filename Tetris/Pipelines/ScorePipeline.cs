using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using BlockArena.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlockArena.Interactors
{
    public class ScorePipeline(IRatingUpdater ratingUpdater, Func<Task<Rating>> getRating) : IScorePipeline
    {
        private readonly Func<Task<Rating>> getRating = getRating;
        private readonly IRatingUpdater ratingUpdater = ratingUpdater;

        public async Task Add(Models.Score userScore)
        {
            await ratingUpdater.Add(new UserScore
            {
                Score = userScore.Count,
                Username = userScore.Username
            });
        }

        public async Task<List<Models.Score>> GetScores(int count)
        {
            return (await getRating())
                .UserScores
                .OrderByDescending(userScore => userScore.Score)
                .Take(count)
                .Select(userScore => new Models.Score
                {
                    Username = userScore.Username,
                    Count = userScore.Score
                })
                .ToList();
        }
    }
}