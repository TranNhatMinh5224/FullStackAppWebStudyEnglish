using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Lessons;

public class ModuleTests
{
    [Fact]
    public void Constructor_ShouldSetDefaultValues()
    {
        var module = new Module();
        Assert.NotNull(module.Lectures);
        Assert.NotNull(module.FlashCards);
        Assert.NotNull(module.Assessments);
        Assert.NotNull(module.ModuleCompletions);
        Assert.NotEqual(default(DateTime), module.CreatedAt);
    }
}
