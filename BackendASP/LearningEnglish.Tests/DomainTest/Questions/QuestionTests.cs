using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Questions;

public class QuestionTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var question = new Question();

        // Assert
        Assert.Equal(QuestionType.MultipleChoice, question.Type);
        Assert.Equal(10m, question.Points);
        Assert.Equal("{}", question.MetadataJson);
        Assert.Equal(0, question.DisplayOrder);
        Assert.NotNull(question.Options);
        Assert.Empty(question.Options);
    }
}
