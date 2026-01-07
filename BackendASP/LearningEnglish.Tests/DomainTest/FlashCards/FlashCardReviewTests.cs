using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.FlashCards;

public class FlashCardReviewTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultSpacedRepetitionValues()
    {
        // Act
        var review = new FlashCardReview();

        // Assert
        Assert.Equal(2.5f, review.EasinessFactor);
        Assert.Equal(1, review.IntervalDays);
        Assert.Equal(0, review.RepetitionCount);
        Assert.Equal(0, review.Quality);
    }
}
