using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Quizzes;

public class QuizTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var quiz = new Quiz();

        // Assert
        Assert.Equal(QuizType.Practice, quiz.Type);
        Assert.Equal(QuizStatus.Open, quiz.Status);
        Assert.True(quiz.ShowAnswersAfterSubmit);
        Assert.True(quiz.ShowScoreImmediately);
        Assert.True(quiz.ShuffleQuestions);
        Assert.True(quiz.ShuffleAnswers);
        Assert.NotNull(quiz.QuizSections);
        Assert.Empty(quiz.QuizSections);
        Assert.NotNull(quiz.Attempts);
        Assert.Empty(quiz.Attempts);
    }
}
