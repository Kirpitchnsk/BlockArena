using System.Threading.Tasks;
using StackExchange.Redis;
using BlockArena.Domain.Interfaces;
using BlockArena.Domain.Models;

namespace BlockArena.Database
{
    public class RedisRatingStorage(IConnectionMultiplexer redis) : IRatingStorage
    {
        private readonly IDatabase db = redis.GetDatabase();

        public async Task Add(UserScore userScore)
        {
            await db.SortedSetAddAsync("user", userScore.Username, userScore.Score);
        }
    }
}
