using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class RoleTests
{
    [Fact]
    public void Constructor_ShouldInitializeCollections()
    {
        var role = new Role();
        Assert.NotNull(role.Users);
        Assert.NotNull(role.RolePermissions);
    }
}
