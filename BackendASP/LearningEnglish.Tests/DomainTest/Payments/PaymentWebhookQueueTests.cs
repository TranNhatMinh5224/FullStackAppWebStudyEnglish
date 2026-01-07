using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Payments;

public class PaymentWebhookQueueTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var queue = new PaymentWebhookQueue();
        Assert.Equal(WebhookStatus.Pending, queue.Status);
        Assert.Equal(0, queue.RetryCount);
        Assert.Equal(5, queue.MaxRetries);
        Assert.NotEqual(default(DateTime), queue.CreatedAt);
    }
}
