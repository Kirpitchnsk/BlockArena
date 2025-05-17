using System.Threading.Tasks;
using StackExchange.Redis;
using BlockArena.Common.Models;
using BlockArena.Common.Interfaces;

namespace BlockArena.Database
{
    public class RedisRatingStorage(IConnectionMultiplexer redisClient) : IRatingStorage
    {
        private readonly IDatabase db = redisClient.GetDatabase();

        public async Task Add(UserScore userScore)
        {
            await db.SortedSetAddAsync("user", userScore.Username, userScore.Score);
        }
    }
}
