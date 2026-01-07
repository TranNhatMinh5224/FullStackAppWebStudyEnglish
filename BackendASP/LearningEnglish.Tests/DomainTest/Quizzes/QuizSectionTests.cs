using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Quizzes;

public class QuizSectionTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var section = new QuizSection();
        Assert.NotNull(section.QuizGroups);
        Assert.NotNull(section.Questions);
        Assert.NotEqual(default(DateTime), section.CreatedAt);
    }
}
