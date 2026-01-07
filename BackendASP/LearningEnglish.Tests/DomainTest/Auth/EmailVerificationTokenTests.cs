using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class EmailVerificationTokenTests
{
    [Fact]
    public void IsExpired_ShouldReturnTrue_WhenExpiresAtIsPast()
    {
        // Arrange
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5)
        };

        // Act & Assert
        Assert.True(token.IsExpired());
    }

    [Fact]
    public void IsExpired_ShouldReturnFalse_WhenExpiresAtIsFuture()
    {
        // Arrange
        var token = new EmailVerificationToken
        {
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act & Assert
        Assert.False(token.IsExpired());
    }

    [Fact]
    public void IsUsed_DefaultValue_ShouldBeFalse()
    {
        // Arrange
        var token = new EmailVerificationToken();

        // Assert
        Assert.False(token.IsUsed);
    }
}
