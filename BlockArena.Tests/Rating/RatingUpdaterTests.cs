using BlockArena.Common.Exceptions;
using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using BlockArena.Common.Ratings;
using FluentAssertions;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BlockArena.Tests.LeaderBoard
{
    public class RatingUpdaterTests
    {
        private readonly IRatingUpdater ratingUpdater;
        private readonly IRatingStorage scoreBoardStorage;
        private readonly Rating rating;

        public RatingUpdaterTests()
        {
            rating = new Models.Rating();
            scoreBoardStorage = Substitute.For<IRatingStorage>();
            ratingUpdater = new RatingUpdater(
                scoreBoardStorage,
                () => Task.FromResult(rating));
        }

        [Fact]
        public async Task AddsTrimmedNewUserRecord()
        {
            //Arrange
            var userScore = new UserScore { Score = 10, Username = "Stewie                                         " };
            UserScore receivedUserScore = null;
            scoreBoardStorage
                .When(storage => storage.Add(Arg.Any<UserScore>()))
                .Do(ci => receivedUserScore = ci.Arg<UserScore>());

            //Act
            await ratingUpdater.Add(userScore);

            //Assert
            receivedUserScore.Should().BeEquivalentTo(new UserScore { Score = 10, Username = "Stewie" });
        }

        [Fact]
        public async Task DoesNotAddScoreForUserThatExistsWithSameOrHigherScore()
        {
            //Arrange
            rating.UserScores = new List<UserScore> { new UserScore { Score = 10, Username = "stewie" } };

            //Act
            //Assert
            (await ((Func<Task>)(async () => await ratingUpdater.Add(new UserScore { Score = 10, Username = "Stewie" })))
                .Should()
                .ThrowAsync<ValidationException>())
                .WithMessage("Stewie already has a score equal to or greater than 10.");

            await scoreBoardStorage.Received(0).Add(Arg.Any<UserScore>());
        }

        [Fact]
        public async Task DoesNotAddScoreForUsernamesThatAreTooLong()
        {
            //Arrange
            rating.UserScores = new List<UserScore> { new UserScore { Score = 10, Username = "stewie" } };

            //Act
            //Assert
            (await ((Func<Task>)(async () => await ratingUpdater.Add(new UserScore { Score = 10, Username = "some really really long user name here" })))
                .Should()
                .ThrowAsync<ValidationException>())
                .WithMessage("Username length must not be over 20.");

            await scoreBoardStorage.Received(0).Add(Arg.Any<UserScore>());
        }
    }
}
