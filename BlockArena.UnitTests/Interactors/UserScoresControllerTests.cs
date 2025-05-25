using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using BlockArena.Interactors;
using BlockArena.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace BlockArena.UnitTests.Interactors
{
    [TestFixture]
    public class UserScoresControllerTests
    {
        private IScorePipeline? scorePipeline;
        private IRatingUpdater? ratingUpdater;
        private Rating? rating;

        [SetUp]
        public void SetUp()
        {
            rating = new Rating();
            ratingUpdater = Substitute.For<IRatingUpdater>();
            scorePipeline = new ScorePipeline(
                ratingUpdater,
                getRating: () => Task.FromResult(rating));
        }

        [Test]
        public async Task GetUserScores()
        {
            //Arrange
            rating.UserScores = new List<UserScore>
            {
                new UserScore { Username = "Artem", IsBot = true, Score = 102 },
                new UserScore { Username = "Anton", IsBot = true, Score = 99 },
                new UserScore { Username = "Denis", IsBot = true, Score = 100 },
                new UserScore { Username = "masha", IsBot = true, Score = 50 }
            };

            //Act
            //Assert
            (await scorePipeline.GetScores(count: 3)).Should().BeEquivalentTo(new List<Models.UserScore>
            {
                new Models.UserScore { Username = "Artem", Score = 102 },
                new Models.UserScore { Username = "Denis", Score = 100 },
                new Models.UserScore { Username = "Anton", Score = 99 }
            }, ops => ops.WithStrictOrdering());
        }

        [Test]
        public async Task AddUserScore()
        {
            //Arrange
            UserScore? receivedUserScore = null;
            ratingUpdater
                .When(updater => updater.Add(Arg.Any<UserScore>()))
                .Do(ci => receivedUserScore = ci.Arg<UserScore>());

            //Act
            await scorePipeline.Add(new Models.UserScore { Username = "Vlad", Score = 200 });

            //Assert
            receivedUserScore.Should().BeEquivalentTo(new UserScore
            {
                Username = "Vlad",
                Score = 200
            });
        }
    }
}
