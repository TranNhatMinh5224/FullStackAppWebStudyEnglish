using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Assets;

public class AssetFrontendTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        // Act
        var asset = new AssetFrontend();

        // Assert
        Assert.True(asset.IsActive);
        Assert.Equal(0, asset.Order);
        Assert.NotEqual(default(DateTime), asset.CreatedAt);
    }
}
