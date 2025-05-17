using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using StackExchange.Redis;
using System.Linq;
using System.Threading.Tasks;

namespace BlockArena.Database
{
    public class RedisRatingProvider(IConnectionMultiplexer redisClient) : IRatingHandler
    {
        public int MaxScores { get; set; } = 20;

        public async Task<Rating> GetRating()
        {
            var db = redisClient.GetDatabase();
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