using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Essay;

public class EssayTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var essay = new LearningEnglish.Domain.Entities.Essay();
        Assert.NotNull(essay.EssaySubmissions);
        Assert.Empty(essay.EssaySubmissions);
    }
}
