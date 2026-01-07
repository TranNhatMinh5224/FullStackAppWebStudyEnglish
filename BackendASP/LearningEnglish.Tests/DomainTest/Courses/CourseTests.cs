using LearningEnglish.Domain.Entities;
using Xunit;

namespace LearningEnglish.Tests.DomainTest.Courses;

public class CourseTests
{
    [Fact]
    public void IsFree_ShouldReturnTrue_WhenPriceIsNull()
    {
        // Arrange
        var course = new Course { Price = null };

        // Act & Assert
        Assert.True(course.IsFree());
    }

    [Fact]
    public void IsFree_ShouldReturnTrue_WhenPriceIsZero()
    {
        // Arrange
        var course = new Course { Price = 0m };

        // Act & Assert
        Assert.True(course.IsFree());
    }

    [Fact]
    public void IsFree_ShouldReturnFalse_WhenPriceIsGreaterThanZero()
    {
        // Arrange
        var course = new Course { Price = 10.5m };

        // Act & Assert
        Assert.False(course.IsFree());
    }

    [Fact]
    public void CanJoin_ShouldReturnTrue_WhenMaxStudentIsZero()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 0,
            EnrollmentCount = 100 
        };

        // Act & Assert
        Assert.True(course.CanJoin());
    }

    [Fact]
    public void CanJoin_ShouldReturnTrue_WhenEnrollmentCountIsLessThanMax()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 10,
            EnrollmentCount = 9 
        };

        // Act & Assert
        Assert.True(course.CanJoin());
    }

    [Fact]
    public void CanJoin_ShouldReturnFalse_WhenEnrollmentCountEqualsMax()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 10,
            EnrollmentCount = 10 
        };

        // Act & Assert
        Assert.False(course.CanJoin());
    }

    [Fact]
    public void EnrollStudent_ShouldIncrementCount_WhenCourseCanBeJoined()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 10,
            EnrollmentCount = 5 
        };

        // Act
        course.EnrollStudent();

        // Assert
        Assert.Equal(6, course.EnrollmentCount);
    }

    [Fact]
    public void EnrollStudent_ShouldThrowException_WhenCourseIsFull()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 10,
            EnrollmentCount = 10 
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => course.EnrollStudent());
        Assert.Equal("Cannot enroll more students, maximum capacity reached.", exception.Message);
    }

    [Fact]
    public void EnrollStudent_ShouldIncrementCount_WhenMaxStudentIsZero()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 0,
            EnrollmentCount = 100 
        };

        // Act
        course.EnrollStudent();

        // Assert
        Assert.Equal(101, course.EnrollmentCount);
    }

    [Fact]
    public void UnenrollStudent_ShouldDecrementCount_WhenCountIsPositive()
    {
        // Arrange
        var course = new Course { EnrollmentCount = 5 };

        // Act
        course.UnenrollStudent();

        // Assert
        Assert.Equal(4, course.EnrollmentCount);
    }

    [Fact]
    public void UnenrollStudent_ShouldNotDecrementCount_WhenCountIsZero()
    {
        // Arrange
        var course = new Course { EnrollmentCount = 0 };

        // Act
        course.UnenrollStudent();

        // Assert
        Assert.Equal(0, course.EnrollmentCount);
    }
}
