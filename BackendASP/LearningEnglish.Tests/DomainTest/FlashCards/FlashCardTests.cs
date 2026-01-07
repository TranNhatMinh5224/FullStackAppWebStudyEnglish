using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.FlashCards;

public class FlashCardTests
{
    [Fact]
    public void Constructor_ShouldInitializeCollections()
    {
        // Act
        var flashCard = new FlashCard();

        // Assert
        Assert.NotNull(flashCard.Reviews);
        Assert.Empty(flashCard.Reviews);
        Assert.NotNull(flashCard.PronunciationProgresses);
        Assert.Empty(flashCard.PronunciationProgresses);
    }
    
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var flashCard = new FlashCard();

        // Assert
        Assert.Equal("image", flashCard.ImageType);
        Assert.Equal("audio", flashCard.AudioType);
        Assert.NotEqual(default(DateTime), flashCard.CreatedAt);
    }
}
