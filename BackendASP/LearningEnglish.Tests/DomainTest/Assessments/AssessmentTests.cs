using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Assessments;

public class AssessmentTests
{
    [Fact]
    public void Constructor_ShouldInitializeCollectionsAndDefaults()
    {
        // Act
        var assessment = new Assessment();

        // Assert
        Assert.NotNull(assessment.Essays);
        Assert.NotNull(assessment.Quizzes);
        Assert.True(assessment.IsPublished);
    }
}
