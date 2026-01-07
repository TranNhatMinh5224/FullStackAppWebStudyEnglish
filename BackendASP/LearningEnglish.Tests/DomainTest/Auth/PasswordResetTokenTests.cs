using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class PasswordResetTokenTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var token = new PasswordResetToken();

        // Assert
        Assert.False(token.IsUsed);
        Assert.Equal(0, token.AttemptsCount);
        Assert.Null(token.BlockedUntil);
        Assert.NotEqual(default(DateTime), token.CreatedAt);
    }
}
