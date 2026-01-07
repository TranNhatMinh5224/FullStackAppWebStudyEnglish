using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Quizzes;

public class QuizAttemptTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var attempt = new QuizAttempt();

        // Assert
        Assert.Equal(QuizAttemptStatus.InProgress, attempt.Status);
        Assert.Equal(0, attempt.TotalScore);
        Assert.NotEqual(default(DateTime), attempt.StartedAt);
        Assert.Null(attempt.SubmittedAt);
    }
}
