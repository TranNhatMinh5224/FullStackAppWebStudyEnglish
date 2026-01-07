using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Lectures;

public class LectureTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var lecture = new Lecture();

        // Assert
        Assert.Equal(LectureType.Content, lecture.Type);
        Assert.NotNull(lecture.Children);
        Assert.Empty(lecture.Children);
        Assert.NotEqual(default(DateTime), lecture.CreatedAt);
    }
}
