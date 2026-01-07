using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Lessons;

public class LessonCompletionTests
{
    [Fact]
    public void UpdateModuleProgress_ShouldCalculatePercentageCorrectly()
    {
        // Arrange
        var completion = new LessonCompletion();
        
        // Act
        completion.UpdateModuleProgress(10, 5);

        // Assert
        Assert.Equal(50f, completion.CompletionPercentage);
        Assert.Equal(10, completion.TotalModules);
        Assert.Equal(5, completion.CompletedModules);
    }

    [Fact]
    public void UpdateModuleProgress_ShouldSetCompleted_WhenPercentageIsEightyOrMore()
    {
        // Arrange
        var completion = new LessonCompletion();
        
        // Act
        completion.UpdateModuleProgress(10, 8); // 80%

        // Assert
        Assert.True(completion.IsCompleted);
        Assert.NotNull(completion.CompletedAt);
    }

    [Fact]
    public void UpdateModuleProgress_ShouldResetCompleted_WhenPercentageDrops()
    {
        // Arrange
        var completion = new LessonCompletion();
        completion.UpdateModuleProgress(10, 8); // Completed first
        Assert.True(completion.IsCompleted);

        // Act
        completion.UpdateModuleProgress(10, 7); // Drop to 70%

        // Assert
        Assert.False(completion.IsCompleted);
        Assert.Null(completion.CompletedAt);
    }

    [Fact]
    public void MarkVideoProgress_ShouldUpdateProperties()
    {
        // Arrange
        var completion = new LessonCompletion();
        
        // Act
        completion.MarkVideoProgress(120, 50.5f);

        // Assert
        Assert.Equal(120, completion.LastWatchedPositionSeconds);
        Assert.Equal(50.5f, completion.VideoProgressPercentage);
        Assert.NotNull(completion.StartedAt);
    }

    [Fact]
    public void GetProgressDisplay_ShouldReturnCorrectFormat()
    {
        // Arrange
        var completion = new LessonCompletion();
        completion.UpdateModuleProgress(10, 5);

        // Act
        var display = completion.GetProgressDisplay();

        // Assert
        Assert.Equal("5/10 modules (50.0%)", display);
    }
}
