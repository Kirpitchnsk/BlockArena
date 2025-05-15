using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using BlockArena.Domain.Interfaces;
using BlockArena.Domain.Models;

namespace BlockArena.Database
{
    public class RedisRatingProvider(IConnectionMultiplexer redis) : IRatingHandler
    {
        public int MaxScores { get; set; } = 20;

        public async Task<Rating> GetRating()
        {
            var db = redis.GetDatabase();
            var entries = await db.SortedSetRangeByRankWithScoresAsync("user", 0, MaxScores - 1, Order.Descending);

            return new Rating
            {
                UserScores = entries
                    .Select(userScore => new UserScore { Score = (int)userScore.Score, Username = userScore.Element })
                    .ToList()
            };
        }
    }
}