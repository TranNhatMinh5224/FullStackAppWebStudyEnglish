using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Payments;

public class PaymentTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var payment = new Payment();

        // Assert
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(PaymentGateway.PayOs, payment.Gateway);
        Assert.NotEqual(default(DateTime), payment.CreatedAt);
    }
}
