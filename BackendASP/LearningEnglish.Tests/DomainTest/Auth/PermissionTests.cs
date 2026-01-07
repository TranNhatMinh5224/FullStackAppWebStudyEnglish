using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class PermissionTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var permission = new Permission();
        Assert.NotEqual(default(DateTime), permission.CreatedAt);
        Assert.NotNull(permission.RolePermissions);
    }
}
