using LearningEnglish.Domain.Entities;
using Xunit;
using System.Text.Json;

namespace LearningEnglish.Tests.DomainTest.Pronunciation;

public class PronunciationProgressTests
{
    [Fact]
    public void UpdateAfterAssessment_FirstAttempt_ShouldSetValuesCorrectly()
    {
        // Arrange
        var progress = new PronunciationProgress();
        var time = DateTime.UtcNow;

        // Act
        progress.UpdateAfterAssessment(
            accuracyScore: 80,
            fluencyScore: 70,
            completenessScore: 90,
            pronunciationScore: 85,
            problemPhonemes: new List<string> { "th" },
            strongPhonemes: new List<string> { "a" },
            attemptTime: time
        );

        // Assert
        Assert.Equal(1, progress.TotalAttempts);
        Assert.Equal(80, progress.AvgAccuracyScore);
        Assert.Equal(85, progress.BestScore);
        Assert.Equal(time, progress.LastPracticedAt);
        Assert.Equal(1, progress.ConsecutiveDaysStreak);
        Assert.False(progress.IsMastered);
    }

    [Fact]
    public void UpdateAfterAssessment_SecondAttempt_ShouldCalculateRollingAverage()
    {
        // Arrange
        var progress = new PronunciationProgress();
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), DateTime.UtcNow); // First attempt

        // Act
        progress.UpdateAfterAssessment(
            accuracyScore: 90,
            fluencyScore: 90,
            completenessScore: 90,
            pronunciationScore: 90,
            problemPhonemes: new(),
            strongPhonemes: new(),
            attemptTime: DateTime.UtcNow
        );

        // Assert
        Assert.Equal(2, progress.TotalAttempts);
        Assert.Equal(85, progress.AvgAccuracyScore); // (80 + 90) / 2 = 85
        Assert.Equal(85, progress.AvgFluencyScore);
        Assert.Equal(90, progress.BestScore);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldUpdateBestScore_WhenNewScoreIsHigher()
    {
        // Arrange
        var progress = new PronunciationProgress();
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), DateTime.UtcNow); // Best: 80

        // Act
        progress.UpdateAfterAssessment(90, 90, 90, 90, new(), new(), DateTime.UtcNow); // New: 90

        // Assert
        Assert.Equal(90, progress.BestScore);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldNotUpdateBestScore_WhenNewScoreIsLower()
    {
        // Arrange
        var progress = new PronunciationProgress();
        progress.UpdateAfterAssessment(90, 90, 90, 90, new(), new(), DateTime.UtcNow); // Best: 90

        // Act
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), DateTime.UtcNow); // New: 80

        // Assert
        Assert.Equal(90, progress.BestScore);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldIncrementStreak_WhenPracticeIsConsecutiveDay()
    {
        // Arrange
        var progress = new PronunciationProgress();
        var day1 = DateTime.UtcNow.Date;
        var day2 = day1.AddDays(1);

        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), day1);

        // Act
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), day2);

        // Assert
        Assert.Equal(2, progress.ConsecutiveDaysStreak);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldResetStreak_WhenPracticeIsNotConsecutive()
    {
        // Arrange
        var progress = new PronunciationProgress();
        var day1 = DateTime.UtcNow.Date;
        var day3 = day1.AddDays(2); // Missed one day

        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), day1);

        // Act
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), day3);

        // Assert
        Assert.Equal(1, progress.ConsecutiveDaysStreak);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldMaintainStreak_WhenPracticeIsSameDay()
    {
        // Arrange
        var progress = new PronunciationProgress();
        var day1 = DateTime.UtcNow.Date;
        
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), day1);
        Assert.Equal(1, progress.ConsecutiveDaysStreak);

        // Act
        progress.UpdateAfterAssessment(80, 80, 80, 80, new(), new(), day1.AddHours(2));

        // Assert
        Assert.Equal(1, progress.ConsecutiveDaysStreak);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldMarkMastered_WhenCriteriaMet()
    {
        // Arrange
        var progress = new PronunciationProgress
        {
            BestScore = 89,
            AvgPronunciationScore = 84,
            IsMastered = false
        };

        // Act - Update to meet criteria: Best >= 90 AND Avg >= 85
        // Need to push Avg from 84 to 85. Suppose TotalAttempts = 1.
        // New Avg = (84*1 + 86)/2 = 85.
        // New Best = 90.
        
        // Simulating manual setup for test simplicity (as rolling avg depends on TotalAttempts)
        progress.TotalAttempts = 1;
        progress.UpdateAfterAssessment(
            accuracyScore: 90, 
            fluencyScore: 90, 
            completenessScore: 90, 
            pronunciationScore: 92, // High score
            problemPhonemes: new(), 
            strongPhonemes: new(), 
            attemptTime: DateTime.UtcNow
        );

        // Assert
        Assert.True(progress.BestScore >= 90);
        Assert.True(progress.AvgPronunciationScore >= 85);
        Assert.True(progress.IsMastered);
        Assert.NotNull(progress.MasteredAt);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldNotMarkMastered_WhenAvgScoreIsBelowThreshold()
    {
        // Arrange
        var progress = new PronunciationProgress
        {
            TotalAttempts = 1,
            BestScore = 95,
            AvgPronunciationScore = 80,
            IsMastered = false
        };

        // Act
        // Current Avg 80. New Attempt 89. New Avg = (80+89)/2 = 84.5 < 85
        progress.UpdateAfterAssessment(89, 89, 89, 89, new(), new(), DateTime.UtcNow);

        // Assert
        Assert.Equal(84.5, progress.AvgPronunciationScore);
        Assert.False(progress.IsMastered);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldNotMarkMastered_WhenBestScoreIsBelowThreshold()
    {
        // Arrange
        var progress = new PronunciationProgress
        {
            TotalAttempts = 1,
            BestScore = 80,
            AvgPronunciationScore = 84,
            IsMastered = false
        };

        // Act
        // Current Avg 84. New Attempt 88. New Avg = (84+88)/2 = 86 > 85 (OK)
        // Best Score updates to 88 < 90 (FAIL)
        progress.UpdateAfterAssessment(88, 88, 88, 88, new(), new(), DateTime.UtcNow);

        // Assert
        Assert.True(progress.AvgPronunciationScore >= 85);
        Assert.True(progress.BestScore < 90);
        Assert.False(progress.IsMastered);
    }

    [Fact]
    public void UpdateAfterAssessment_ShouldNotUpdateMasteredAt_WhenAlreadyMastered()
    {
        // Arrange
        var initialMasteredDate = DateTime.UtcNow.AddDays(-1);
        var progress = new PronunciationProgress
        {
            IsMastered = true,
            MasteredAt = initialMasteredDate,
            BestScore = 95,
            AvgPronunciationScore = 90
        };

        // Act
        progress.UpdateAfterAssessment(95, 95, 95, 95, new(), new(), DateTime.UtcNow);

        // Assert
        Assert.Equal(initialMasteredDate, progress.MasteredAt);
    }
}
