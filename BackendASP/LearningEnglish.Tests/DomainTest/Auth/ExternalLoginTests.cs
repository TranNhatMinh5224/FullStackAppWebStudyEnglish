using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class ExternalLoginTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var login = new ExternalLogin();
        Assert.NotEqual(default(DateTime), login.CreatedAt);
    }
}
