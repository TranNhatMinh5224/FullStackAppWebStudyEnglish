using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Lessons;

public class LessonTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var lesson = new Lesson();
        Assert.NotNull(lesson.Modules);
        Assert.Empty(lesson.Modules);
        Assert.NotNull(lesson.LessonCompletions);
        Assert.Empty(lesson.LessonCompletions);
        Assert.NotEqual(default(DateTime), lesson.CreatedAt);
    }
}
