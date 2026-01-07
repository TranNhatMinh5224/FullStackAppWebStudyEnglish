using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class RefreshTokenTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var token = new RefreshToken();
        Assert.False(token.IsRevoked);
        Assert.NotEqual(default(DateTime), token.CreatedAt);
    }
}
