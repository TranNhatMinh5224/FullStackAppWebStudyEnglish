using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Subscriptions;

public class TeacherSubscriptionTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var subscription = new TeacherSubscription();

        // Assert
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
        Assert.False(subscription.AutoRenew);
        Assert.NotEqual(default(DateTime), subscription.CreatedAt);
    }
}
