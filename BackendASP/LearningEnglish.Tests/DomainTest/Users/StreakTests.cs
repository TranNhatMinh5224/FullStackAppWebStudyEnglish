using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Users;

public class StreakTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var streak = new Streak();
        Assert.Equal(0, streak.CurrentStreak);
        Assert.Equal(0, streak.LongestStreak);
        Assert.Equal(0, streak.TotalActiveDays);
        Assert.NotEqual(default(DateTime), streak.CreatedAt);
    }
}
