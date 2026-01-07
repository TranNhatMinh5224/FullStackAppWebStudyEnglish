using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Users;

public class UserCourseTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var userCourse = new UserCourse();
        Assert.NotEqual(default(DateTime), userCourse.JoinedAt);
    }
}
