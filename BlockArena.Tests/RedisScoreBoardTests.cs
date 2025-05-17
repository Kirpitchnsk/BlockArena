using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using StackExchange.Redis;
using Xunit;
using BlockArena.Common;
using BlockArena.Common.Models;
using BlockArena.Database;

namespace BlockArena.Tests
{
    public class RedisScoreBoardTests
    {
        private readonly Task<ConnectionMultiplexer> redisConfig;

        public RedisScoreBoardTests()
        {
            redisConfig = ConnectionMultiplexer.ConnectAsync(
                TestConfigContainer.GetConfig()["RedisConnectionString"]
                );
        }

        [Theory]
        [InlineData(0)]
        public async Task StoresTheScore(int start)
        {
            var db = (await redisConfig).GetDatabase();

            var userScores = new List<UserScore>{
                new UserScore { Username = $"user {start}", Score = 10 },
                new UserScore { Username = $"user {start + 1}", Score = 3 },
                new UserScore { Username = $"user {start + 2}", Score = 1 },
                new UserScore { Username = $"user {start + 3}", Score = 5 },
                new UserScore { Username = $"user {start + 4}", Score = 2 }
            };
            var leaderBoardProvider = new RedisRatingProvider(redisConfig) { MaxScores = 10000 };
            var scoreBoard = new RedisRatingStorage(redisConfig);

            await Task.WhenAll(userScores
                .Select(userScore => db.SortedSetRemoveAsync("user", userScore.Username)));
            (await leaderBoardProvider.GetRating())
                .UserScores
                .Should()
                .NotContain(userScores);

            await Task.WhenAll(userScores.Select(score => scoreBoard.Add(score)));
            var recordedScores = (await leaderBoardProvider.GetRating()).UserScores;
            userScores.ForEach(score => recordedScores
                .Should()
                .ContainEquivalentOf(score));
            await Task.WhenAll(userScores
                .Select(userScore => db.SortedSetRemoveAsync("user", userScore.Username)));
        }

        [Fact]
        public async Task LoadTest()
        {
            const int count = 5000;
            var db = (await redisConfig).GetDatabase();
            var usernames = Enumerable
                .Range(start: 0, count: count + 5)
                .Select(i => $"user {i}")
                .ToList();

            await Task.WhenAll(usernames
                .Select(username => db.SortedSetRemoveAsync("user", username)));

            await TaskHelper.WhenAll(Enumerable
                .Range(start: 1, count: count)
                .Select(i => (Func<Task>)(() => StoresTheScore(start: i * 10))),
                max: 100);
        }
    }
}