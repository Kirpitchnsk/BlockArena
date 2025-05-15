using FluentAssertions;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using BlockArena.Domain.Interfaces;
using BlockArena.Domain.Models;
using BlockArena.Interactors;
using BlockArena.Interfaces;
using Xunit;

namespace BlockArena.Tests.Interactors
{
    public class UserScoresControllerTests
    {
        private readonly IScorePipeline userScoresInteractor;
        private readonly IRatingUpdater leaderBoardUpdater;
        private readonly Rating rating;

        public UserScoresControllerTests()
        {
            rating = new Rating();
            leaderBoardUpdater = Substitute.For<IRatingUpdater>();
            userScoresInteractor = new ScorePipeline(
                leaderBoardUpdater,
                getRating: () => Task.FromResult(rating));
        }

        [Fact]
        public async Task GetuserScores()
        {
            //Arrange
            rating.UserScores = new List<UserScore>
            {
                new UserScore { Username = "Stewie", IsBot = true, Score = 102 },
                new UserScore { Username = "Max", IsBot = true, Score = 99 },
                new UserScore { Username = "John", IsBot = true, Score = 100 },
                new UserScore { Username = "Chris", IsBot = true, Score = 50 }
            };

            //Act
            //Assert
            (await userScoresInteractor.GetUserScores(count: 3)).Should().BeEquivalentTo(new List<Models.UserScore>
            {
                new Models.UserScore { Username = "Stewie", Score = 102 },
                new Models.UserScore { Username = "John", Score = 100 },
                new Models.UserScore { Username = "Max", Score = 99 }
            }, ops => ops.WithStrictOrdering());
        }

        [Fact]
        public async Task AddUserScore()
        {
            //Arrange
            UserScore receivedUserScore = null;
            leaderBoardUpdater
                .When(updater => updater.Add(Arg.Any<UserScore>()))
                .Do(ci => receivedUserScore = ci.Arg<UserScore>());

            //Act
            await userScoresInteractor.Add(new Models.UserScore { Username = "Stewie", Score = 200 });

            //Assert
            receivedUserScore.Should().BeEquivalentTo(new UserScore
            {
                Username = "Stewie",
                Score = 200
            });
        }
    }
}
