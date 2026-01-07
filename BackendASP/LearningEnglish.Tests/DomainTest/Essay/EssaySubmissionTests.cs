using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Essay;

public class EssaySubmissionTests
{
    [Fact]
    public void FinalScore_ShouldReturnAiScore_WhenTeacherScoreIsNull()
    {
        // Arrange
        var submission = new EssaySubmission
        {
            Score = 75.5m,
            TeacherScore = null
        };

        // Assert
        Assert.Equal(75.5m, submission.FinalScore);
    }

    [Fact]
    public void FinalScore_ShouldReturnTeacherScore_WhenTeacherScoreIsSet()
    {
        // Arrange
        var submission = new EssaySubmission
        {
            Score = 75.5m,
            TeacherScore = 80m
        };

        // Assert
        Assert.Equal(80m, submission.FinalScore);
    }

    [Fact]
    public void FinalScore_ShouldReturnNull_WhenBothScoresAreNull()
    {
        // Arrange
        var submission = new EssaySubmission
        {
            Score = null,
            TeacherScore = null
        };

        // Assert
        Assert.Null(submission.FinalScore);
    }
}
