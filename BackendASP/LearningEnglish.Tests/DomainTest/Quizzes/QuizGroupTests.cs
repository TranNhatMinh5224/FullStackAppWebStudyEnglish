using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Quizzes;

public class QuizGroupTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var group = new QuizGroup();
        Assert.Equal(0, group.DisplayOrder);
        Assert.NotNull(group.Questions);
        Assert.NotEqual(default(DateTime), group.CreatedAt);
    }
}
