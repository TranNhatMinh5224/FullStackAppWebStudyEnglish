using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Notifications;

public class NotificationTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var notification = new Notification();

        // Assert
        Assert.False(notification.IsRead);
        Assert.False(notification.IsEmailSent);
        Assert.NotEqual(default(DateTime), notification.CreatedAt);
    }
}
