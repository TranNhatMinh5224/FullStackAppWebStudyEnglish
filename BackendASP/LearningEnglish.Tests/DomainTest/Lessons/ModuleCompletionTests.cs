using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Lessons;

public class ModuleCompletionTests
{
    [Fact]
    public void MarkAsCompleted_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var completion = new ModuleCompletion();

        // Act
        completion.MarkAsCompleted();

        // Assert
        Assert.True(completion.IsCompleted);
        Assert.Equal(100, completion.ProgressPercentage);
        Assert.NotEqual(default, completion.CompletedAt);
        Assert.NotNull(completion.StartedAt);
    }

    [Fact]
    public void MarkAsStarted_ShouldInitializeStartedAt_WhenNull()
    {
        // Arrange
        var completion = new ModuleCompletion { StartedAt = null };

        // Act
        completion.MarkAsStarted();

        // Assert
        Assert.NotNull(completion.StartedAt);
        Assert.False(completion.IsCompleted);
        Assert.Equal(0, completion.ProgressPercentage);
    }

    [Fact]
    public void MarkAsStarted_ShouldNotChangeStartedAt_WhenAlreadySet()
    {
        // Arrange
        var initialTime = DateTime.UtcNow.AddHours(-1);
        var completion = new ModuleCompletion { StartedAt = initialTime };

        // Act
        completion.MarkAsStarted();

        // Assert
        Assert.Equal(initialTime, completion.StartedAt);
    }
}
