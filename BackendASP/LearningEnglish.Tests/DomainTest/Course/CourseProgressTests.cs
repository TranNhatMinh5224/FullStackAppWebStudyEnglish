using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Courses;

public class CourseProgressTests
{
    [Fact]
    public void UpdateProgress_ShouldCalculatePercentageCorrectly()
    {
        // Arrange
        var progress = new CourseProgress();
        int totalLessons = 10;
        int completedLessons = 5;

        // Act
        progress.UpdateProgress(totalLessons, completedLessons);

        // Assert
        Assert.Equal(10, progress.TotalLessons);
        Assert.Equal(5, progress.CompletedLessons);
        Assert.Equal(50m, progress.ProgressPercentage);
    }

    [Fact]
    public void UpdateProgress_ShouldHandleZeroTotalLessons()
    {
        // Arrange
        var progress = new CourseProgress();
        int totalLessons = 0;
        int completedLessons = 0;

        // Act
        progress.UpdateProgress(totalLessons, completedLessons);

        // Assert
        Assert.Equal(0, progress.TotalLessons);
        Assert.Equal(0, progress.CompletedLessons);
        Assert.Equal(0m, progress.ProgressPercentage);
    }

    [Fact]
    public void UpdateProgress_ShouldSetCompletedAt_WhenProgressIsEightyOrMore()
    {
        // Arrange
        var progress = new CourseProgress();
        int totalLessons = 10;
        int completedLessons = 8; // 80%

        // Act
        progress.UpdateProgress(totalLessons, completedLessons);

        // Assert
        Assert.Equal(80m, progress.ProgressPercentage);
        Assert.True(progress.IsCompleted);
        Assert.NotNull(progress.CompletedAt);
    }

    [Fact]
    public void UpdateProgress_ShouldNotResetCompletedAt_WhenAlreadyCompletedAndProgressIncreases()
    {
        // Arrange
        var progress = new CourseProgress();
        progress.UpdateProgress(10, 8); // 80%
        var completedAt = progress.CompletedAt;
        
        // Wait a bit to ensure timestamp difference if it were to change (though in unit test execution is fast)
        // Act
        progress.UpdateProgress(10, 9); // 90%

        // Assert
        Assert.Equal(completedAt, progress.CompletedAt); // Should remain the same initial completion time
    }

    [Fact]
    public void UpdateProgress_ShouldResetCompletedAt_WhenProgressDropsBelowThreshold()
    {
        // Arrange
        var progress = new CourseProgress();
        progress.UpdateProgress(10, 8); // 80% - Completed
        Assert.NotNull(progress.CompletedAt);

        // Act
        progress.UpdateProgress(10, 7); // 70% - Not Completed

        // Assert
        Assert.False(progress.IsCompleted);
        Assert.Null(progress.CompletedAt);
    }

    [Fact]
    public void IsCompleted_ShouldReturnTrue_WhenPercentageIsEightyOrMore()
    {
        // Arrange
        var progress = new CourseProgress { ProgressPercentage = 80m };

        // Act & Assert
        Assert.True(progress.IsCompleted);
    }

    [Fact]
    public void IsCompleted_ShouldReturnFalse_WhenPercentageIsLessThanEighty()
    {
        // Arrange
        var progress = new CourseProgress { ProgressPercentage = 79.9m };

        // Act & Assert
        Assert.False(progress.IsCompleted);
    }

    [Fact]
    public void GetProgressDisplay_ShouldReturnFormattedString()
    {
        // Arrange
        var progress = new CourseProgress
        {
            TotalLessons = 10,
            CompletedLessons = 5,
            ProgressPercentage = 50.0m
        };

        // Act
        var display = progress.GetProgressDisplay();

        // Assert
        Assert.Equal("5/10 (50.0%)", display);
    }
}
