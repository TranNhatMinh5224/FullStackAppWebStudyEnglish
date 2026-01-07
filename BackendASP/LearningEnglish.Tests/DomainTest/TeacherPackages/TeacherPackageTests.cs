using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.TeacherPackages;

public class TeacherPackageTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var package = new TeacherPackage();

        // Assert
        Assert.Equal(12, package.DurationMonths);
        Assert.NotNull(package.Subscriptions);
        Assert.Empty(package.Subscriptions);
    }
}
