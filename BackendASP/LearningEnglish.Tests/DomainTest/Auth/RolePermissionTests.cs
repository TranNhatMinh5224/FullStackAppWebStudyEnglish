using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Auth;

public class RolePermissionTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var rolePermission = new RolePermission();
        Assert.NotEqual(default(DateTime), rolePermission.AssignedAt);
    }
}
