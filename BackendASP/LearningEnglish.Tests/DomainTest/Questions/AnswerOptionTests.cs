using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Questions;

public class AnswerOptionTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var option = new AnswerOption();
        Assert.False(option.IsCorrect);
    }
}
