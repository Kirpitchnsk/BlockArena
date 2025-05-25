using BlockArena.Common.Exceptions;
using BlockArena.Common.Interfaces;
using BlockArena.Common.Models;
using BlockArena.Common.Ratings;
using FluentAssertions;
using NSubstitute;

namespace BlockArena.UnitTests.RatingTests
{
    [TestFixture]
    public class RatingUpdaterTests
    {
        private IRatingUpdater ratingUpdater;
        private IRatingStorage scoreBoardStorage;
        private Rating rating;

        [SetUp]
        public void SetUp()
        {
            rating = new Rating();
            scoreBoardStorage = Substitute.For<IRatingStorage>();
            ratingUpdater = new RatingUpdater(
                scoreBoardStorage,
                () => Task.FromResult(rating));
        }

        [Test]
        public async Task AddsTrimmedNewUserRecord()
        {
            //Arrange
            var userScore = new UserScore { Score = 40, Username = "Vlad                                         " };
            UserScore receivedUserScore = null;
            scoreBoardStorage
                .When(storage => storage.Add(Arg.Any<UserScore>()))
                .Do(ci => receivedUserScore = ci.Arg<UserScore>());

            //Act
            await ratingUpdater.Add(userScore);

            //Assert
            receivedUserScore.Should().BeEquivalentTo(new UserScore { Score = 40, Username = "Vlad" });
        }

        [Test]
        public async Task DoesNotAddScoreForUserThatExistsWithSameOrHigherScore()
        {
            //Arrange
            rating.UserScores = new List<UserScore> { new UserScore { Score = 13, Username = "vlad" } };

            //Act
            //Assert
            (await ((Func<Task>)(async () => await ratingUpdater.Add(new UserScore { Score = 13, Username = "vlad" })))
                .Should()
                .ThrowAsync<ValidationException>())
                .WithMessage("vlad уже имеет рекорд 13.");

            await scoreBoardStorage.Received(0).Add(Arg.Any<UserScore>());
        }

        [Test]
        public async Task DoesNotAddScoreForUsernamesThatAreTooLong()
        {
            //Arrange
            rating.UserScores = new List<UserScore> { new UserScore { Score = 26, Username = "vlad" } };

            //Act
            //Assert
            (await ((Func<Task>)(async () => await ratingUpdater.Add(new UserScore { Score = 26, Username = "максимально огромнейшее имя для проверки здесь" })))
                .Should()
                .ThrowAsync<ValidationException>())
                .WithMessage("Имя не должно быть больше 20.");

            await scoreBoardStorage.Received(0).Add(Arg.Any<UserScore>());
        }
    }
}