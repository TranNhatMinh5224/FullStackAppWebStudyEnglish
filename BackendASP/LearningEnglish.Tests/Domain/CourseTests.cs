using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Tests.Domain;

public class CourseTests
{
    [Fact]
    public void IsFree_WithZeroPrice_ReturnsTrue()
    {
        // Arrange 
        var course = new Course { Price = 0 };

        // Act 
        var result = course.IsFree();

        // Assert 
        Assert.True(result);
    }

    [Fact]
    public void IsFree_WithNullPrice_ReturnsTrue()
    {
        // Arrange 
        var course = new Course { Price = null };

        // Act 
        var result = course.IsFree();

        // Assert 
        Assert.True(result);
    }

    [Fact]
    public void IsFree_WithPriceGreaterThanZero_ReturnsFalse()
    {
        // Arrange 
        var course = new Course { Price = 100000 };

        // Act 
        var result = course.IsFree();

        // Assert 
        Assert.False(result);
    }

    [Fact]
    public void CanJoin_WithNoMaxStudent_ReturnsTrue()
    {
        // Arrange 
        var course = new Course 
        { 
            MaxStudent = 0, 
            EnrollmentCount = 100 
        };

        // Act 
        var result = course.CanJoin();

        // Assert 
        Assert.True(result);
    }

    [Fact]
    public void CanJoin_WithAvailableSlots_ReturnsTrue()
    {
        // Arrange 
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 5 
        };

        // Act 
        var result = course.CanJoin();

        // Assert 
        Assert.True(result);
    }

    [Fact]
    public void CanJoin_WhenFull_ReturnsFalse()
    {
        // Arrange 
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 10 
        };

        // Act 
        var result = course.CanJoin();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanJoin_WhenEnrollmentExceedsMax_ReturnsFalse()
    {
        // Arrange 
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 15 
        };

        // Act 
        var result = course.CanJoin();

        // Assert 
        Assert.False(result);
    }

    [Fact]
    public void EnrollStudent_WithAvailableSlots_IncrementsEnrollmentCount()
    {
        // Arrange 
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 5 
        };
        var initialCount = course.EnrollmentCount;

        // Act 
        course.EnrollStudent();

        // Assert 
        Assert.Equal(initialCount + 1, course.EnrollmentCount);
        Assert.Equal(6, course.EnrollmentCount);
    }

    [Fact]
    public void EnrollStudent_WithNoMaxStudent_IncrementsEnrollmentCount()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 0, 
            EnrollmentCount = 100 
        };
        var initialCount = course.EnrollmentCount;

        // Act 
        course.EnrollStudent();

        // Assert
        Assert.Equal(initialCount + 1, course.EnrollmentCount);
    }

    [Fact]
    public void EnrollStudent_WhenFull_ThrowsException()
    {
        // Arrange 
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 10 
        };

        // Act & Assert - Phải throw exception khi đăng ký thêm
        var exception = Assert.Throws<InvalidOperationException>(() => course.EnrollStudent());
        Assert.Contains("maximum capacity reached", exception.Message);
        Assert.Equal(10, course.EnrollmentCount); // EnrollmentCount không thay đổi
    }

    [Fact]
    public void EnrollStudent_WhenEnrollmentExceedsMax_ThrowsException()
    {
        // Arrange - Số học viên đã vượt quá giới hạn
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 12 
        };

        // Act & Assert - Phải throw exception
        Assert.Throws<InvalidOperationException>(() => course.EnrollStudent());
    }

    [Fact]
    public void UnenrollStudent_WithEnrolledStudents_DecrementsEnrollmentCount()
    {
        // Arrange - Course có học viên
        var course = new Course 
        { 
            EnrollmentCount = 5 
        };
        var initialCount = course.EnrollmentCount;

        // Act - Hủy đăng ký học viên
        course.UnenrollStudent();

        // Assert - Số học viên giảm đi 1
        Assert.Equal(initialCount - 1, course.EnrollmentCount);
        Assert.Equal(4, course.EnrollmentCount);
    }

    [Fact]
    public void UnenrollStudent_WithZeroEnrollment_DoesNotThrow()
    {
        // Arrange - Course không có học viên nào
        var course = new Course 
        { 
            EnrollmentCount = 0 
        };

        // Act - Hủy đăng ký (không có học viên để hủy)
        course.UnenrollStudent();

        // Assert - EnrollmentCount vẫn là 0, không throw exception
        Assert.Equal(0, course.EnrollmentCount);
    }

    [Fact]
    public void UnenrollStudent_MultipleTimes_DoesNotGoBelowZero()
    {
        // Arrange - Course có 1 học viên
        var course = new Course 
        { 
            EnrollmentCount = 1 
        };

        // Act - Hủy đăng ký 2 lần
        course.UnenrollStudent(); // Giảm xuống 0
        course.UnenrollStudent(); // Vẫn là 0, không giảm nữa

        // Assert - EnrollmentCount không bao giờ < 0
        Assert.Equal(0, course.EnrollmentCount);
    }

    [Fact]
    public void EnrollStudent_ThenUnenrollStudent_ReturnsToOriginalCount()
    {
        // Arrange - Course ban đầu có 5 học viên
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 5 
        };
        var originalCount = course.EnrollmentCount;

        // Act - Đăng ký rồi hủy đăng ký
        course.EnrollStudent();
        course.UnenrollStudent();

        // Assert - Trở về số lượng ban đầu
        Assert.Equal(originalCount, course.EnrollmentCount);
    }

    [Fact]
    public void CanJoin_AfterEnrolling_StillReturnsTrueIfNotFull()
    {
        // Arrange - Course có chỗ trống
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 5 
        };

        // Act - Đăng ký 1 học viên
        course.EnrollStudent();

        // Assert - Vẫn còn chỗ
        Assert.True(course.CanJoin());
        Assert.Equal(6, course.EnrollmentCount);
    }

    [Fact]
    public void CanJoin_AfterEnrollingToMax_ReturnsFalse()
    {
        // Arrange - Course còn 1 chỗ
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 9 
        };

        // Act - Đăng ký học viên cuối cùng
        course.EnrollStudent();

        // Assert - Đã đầy, không thể tham gia thêm
        Assert.False(course.CanJoin());
        Assert.Equal(10, course.EnrollmentCount);
    }
}

