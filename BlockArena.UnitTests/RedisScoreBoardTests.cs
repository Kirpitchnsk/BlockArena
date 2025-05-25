using BlockArena.Common;
using BlockArena.Common.Models;
using BlockArena.Database;
using FluentAssertions;
using StackExchange.Redis;

namespace BlockArena.UnitTests
{
    [TestFixture]
    public class RedisScoreBoardTests
    {
        private Task<ConnectionMultiplexer>? redisConfig;

        [SetUp]
        public void SetUp()
        {
            redisConfig = ConnectionMultiplexer.ConnectAsync(
                TestConfigContainer.GetConfig()["RedisConnectionString"]
                );
        }

        [TearDown]
        public async Task TearDown()
        {
            if (redisConfig != null)
            {
                var connection = await redisConfig;
                await connection.DisposeAsync();
            }
        }

        [TestCase(0)]
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

            var leaderBoardProvider = new RedisRatingProvider(await redisConfig) { MaxScores = 10000 };
            var scoreBoard = new RedisRatingStorage(await redisConfig);

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

        [Test]
        public async Task LoadTest()
        {
            var count = 5000;
            var db = (await redisConfig).GetDatabase();
            var usernames = Enumerable
                .Range(0, count + 5)
                .Select(i => $"user {i}")
                .ToList();

            await Task.WhenAll(usernames
                .Select(username => db.SortedSetRemoveAsync("user", username)));

            await TaskHelper.WhenAll(Enumerable
                .Range(1, count)
                .Select(i => (Func<Task>)(() => StoresTheScore(i * 10))),
                100);
        }
    }
}