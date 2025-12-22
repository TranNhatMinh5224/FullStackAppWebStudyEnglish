using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Tests.Domain;

public class CourseTests
{
    // ===== Property Tests =====
    [Fact]
    public void Course_DefaultValues_AreSetCorrectly()
    {
        // Arrange & Act
        var course = new Course();

        // Assert - Kiểm tra các giá trị mặc định
        Assert.Equal(string.Empty, course.Title);
        Assert.Equal(string.Empty, course.DescriptionMarkdown);
        Assert.Equal(CourseType.System, course.Type);
        Assert.Equal(CourseStatus.Draft, course.Status);
        Assert.Equal(0, course.EnrollmentCount);
        Assert.Equal(0, course.MaxStudent);
        Assert.False(course.IsFeatured);
        Assert.NotNull(course.Lessons);
        Assert.Empty(course.Lessons);
        Assert.NotNull(course.UserCourses);
        Assert.Empty(course.UserCourses);
    }

    [Fact]
    public void Course_CreatedAt_IsSetToUtcNow()
    {
        // Arrange & Act
        var beforeCreation = DateTime.UtcNow;
        var course = new Course();
        var afterCreation = DateTime.UtcNow;

        // Assert - CreatedAt phải nằm trong khoảng thời gian tạo
        Assert.True(course.CreatedAt >= beforeCreation);
        Assert.True(course.CreatedAt <= afterCreation);
    }

    [Fact]
    public void Course_PropertiesCanBeSet()
    {
        // Arrange & Act
        var createdDate = DateTime.UtcNow;
        var updatedDate = DateTime.UtcNow.AddDays(1);
        
        var course = new Course
        {
            CourseId = 1,
            Title = "English for Beginners",
            DescriptionMarkdown = "# Introduction\nLearn basic English",
            ImageKey = "course-image-123",
            ImageType = "image/jpeg",
            Type = CourseType.Teacher,
            Status = CourseStatus.Published,
            Price = 500000,
            TeacherId = 10,
            ClassCode = "ENG101",
            CreatedAt = createdDate,
            UpdatedAt = updatedDate,
            EnrollmentCount = 25,
            MaxStudent = 50,
            IsFeatured = true
        };

        // Assert - Tất cả properties phải được set đúng
        Assert.Equal(1, course.CourseId);
        Assert.Equal("English for Beginners", course.Title);
        Assert.Equal("# Introduction\nLearn basic English", course.DescriptionMarkdown);
        Assert.Equal("course-image-123", course.ImageKey);
        Assert.Equal("image/jpeg", course.ImageType);
        Assert.Equal(CourseType.Teacher, course.Type);
        Assert.Equal(CourseStatus.Published, course.Status);
        Assert.Equal(500000, course.Price);
        Assert.Equal(10, course.TeacherId);
        Assert.Equal("ENG101", course.ClassCode);
        Assert.Equal(createdDate, course.CreatedAt);
        Assert.Equal(updatedDate, course.UpdatedAt);
        Assert.Equal(25, course.EnrollmentCount);
        Assert.Equal(50, course.MaxStudent);
        Assert.True(course.IsFeatured);
    }

    [Fact]
    public void Course_WithNullableProperties_CanBeNull()
    {
        // Arrange & Act
        var course = new Course
        {
            Price = null,
            TeacherId = null,
            ClassCode = null,
            UpdatedAt = null,
            ImageKey = null,
            ImageType = null
        };

        // Assert - Các nullable properties có thể là null
        Assert.Null(course.Price);
        Assert.Null(course.TeacherId);
        Assert.Null(course.ClassCode);
        Assert.Null(course.UpdatedAt);
        Assert.Null(course.ImageKey);
        Assert.Null(course.ImageType);
    }

    // ===== CourseType Tests =====
    [Fact]
    public void Course_CanBeSystemType()
    {
        // Arrange & Act
        var course = new Course { Type = CourseType.System };

        // Assert
        Assert.Equal(CourseType.System, course.Type);
    }

    [Fact]
    public void Course_CanBeTeacherType()
    {
        // Arrange & Act
        var course = new Course 
        { 
            Type = CourseType.Teacher,
            TeacherId = 5,
            ClassCode = "TEACH101"
        };

        // Assert
        Assert.Equal(CourseType.Teacher, course.Type);
        Assert.NotNull(course.TeacherId);
        Assert.NotNull(course.ClassCode);
    }

    // ===== CourseStatus Tests =====
    [Fact]
    public void Course_CanBeDraftStatus()
    {
        // Arrange & Act
        var course = new Course { Status = CourseStatus.Draft };

        // Assert
        Assert.Equal(CourseStatus.Draft, course.Status);
    }

    [Fact]
    public void Course_CanBePublishedStatus()
    {
        // Arrange & Act
        var course = new Course { Status = CourseStatus.Published };

        // Assert
        Assert.Equal(CourseStatus.Published, course.Status);
    }

    [Fact]
    public void Course_CanBeArchivedStatus()
    {
        // Arrange & Act
        var course = new Course { Status = CourseStatus.Archived };

        // Assert
        Assert.Equal(CourseStatus.Archived, course.Status);
    }

    // ===== IsFree Tests =====
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
    public void IsFree_WithSmallPrice_ReturnsFalse()
    {
        // Arrange - Giá 1 VND vẫn là trả phí
        var course = new Course { Price = 1 };

        // Act
        var result = course.IsFree();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsFree_WithLargePrice_ReturnsFalse()
    {
        // Arrange - Giá rất cao
        var course = new Course { Price = 10_000_000 };

        // Act
        var result = course.IsFree();

        // Assert
        Assert.False(result);
    }

    // ===== CanJoin Tests =====

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

    // ===== Complex Scenario Tests =====
    [Fact]
    public void Course_MultipleEnrollAndUnenroll_MaintainsCorrectCount()
    {
        // Arrange
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 5 
        };

        // Act - Nhiều thao tác enroll và unenroll
        course.EnrollStudent();  // 6
        course.EnrollStudent();  // 7
        course.UnenrollStudent(); // 6
        course.EnrollStudent();  // 7
        course.EnrollStudent();  // 8
        course.UnenrollStudent(); // 7

        // Assert
        Assert.Equal(7, course.EnrollmentCount);
        Assert.True(course.CanJoin());
    }

    [Fact]
    public void Course_FillToCapacity_ThenUnenroll_AllowsNewEnrollment()
    {
        // Arrange - Đăng ký đến khi đầy
        var course = new Course 
        { 
            MaxStudent = 3, 
            EnrollmentCount = 0 
        };
        
        course.EnrollStudent();
        course.EnrollStudent();
        course.EnrollStudent();
        Assert.False(course.CanJoin());

        // Act - Hủy 1 học viên
        course.UnenrollStudent();

        // Assert - Có thể đăng ký tiếp
        Assert.True(course.CanJoin());
        Assert.Equal(2, course.EnrollmentCount);
    }

    [Fact]
    public void Course_WithUnlimitedCapacity_AcceptsUnlimitedEnrollments()
    {
        // Arrange - MaxStudent = 0 nghĩa là không giới hạn
        var course = new Course 
        { 
            MaxStudent = 0, 
            EnrollmentCount = 0 
        };

        // Act - Đăng ký nhiều học viên
        for (int i = 0; i < 1000; i++)
        {
            Assert.True(course.CanJoin());
            course.EnrollStudent();
        }

        // Assert - Vẫn có thể đăng ký thêm
        Assert.Equal(1000, course.EnrollmentCount);
        Assert.True(course.CanJoin());
    }

    [Fact]
    public void Course_WithZeroEnrollment_CanEnrollFirstStudent()
    {
        // Arrange - Course mới, chưa có ai
        var course = new Course 
        { 
            MaxStudent = 10, 
            EnrollmentCount = 0 
        };

        // Act
        course.EnrollStudent();

        // Assert
        Assert.Equal(1, course.EnrollmentCount);
        Assert.True(course.CanJoin());
    }

    [Fact]
    public void Course_EnrollStudent_ThrowsException_WithCorrectMessage()
    {
        // Arrange - Course đã đầy
        var course = new Course 
        { 
            MaxStudent = 5, 
            EnrollmentCount = 5 
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => course.EnrollStudent());
        Assert.Contains("Cannot enroll", exception.Message);
        Assert.Contains("maximum capacity", exception.Message);
    }

    [Fact]
    public void Course_FreeCourseWithLimitedSlots_WorksCorrectly()
    {
        // Arrange - Course miễn phí nhưng có giới hạn chỗ
        var course = new Course 
        { 
            Price = 0,
            MaxStudent = 5, 
            EnrollmentCount = 4 
        };

        // Act & Assert
        Assert.True(course.IsFree());
        Assert.True(course.CanJoin());
        
        course.EnrollStudent();
        
        Assert.False(course.CanJoin());
        Assert.Equal(5, course.EnrollmentCount);
    }

    [Fact]
    public void Course_PaidCourseWithUnlimitedSlots_WorksCorrectly()
    {
        // Arrange - Course trả phí và không giới hạn
        var course = new Course 
        { 
            Price = 1_000_000,
            MaxStudent = 0, 
            EnrollmentCount = 100 
        };

        // Act & Assert
        Assert.False(course.IsFree());
        Assert.True(course.CanJoin());
        
        course.EnrollStudent();
        
        Assert.True(course.CanJoin());
        Assert.Equal(101, course.EnrollmentCount);
    }

    [Fact]
    public void Course_StatusTransition_WorksCorrectly()
    {
        // Arrange - Course bắt đầu là Draft
        var course = new Course();
        Assert.Equal(CourseStatus.Draft, course.Status);

        // Act - Chuyển sang Published
        course.Status = CourseStatus.Published;
        Assert.Equal(CourseStatus.Published, course.Status);

        // Act - Chuyển sang Archived
        course.Status = CourseStatus.Archived;
        Assert.Equal(CourseStatus.Archived, course.Status);
    }

    [Fact]
    public void Course_WithTeacherAndClassCode_IsTeacherCourse()
    {
        // Arrange & Act
        var course = new Course
        {
            Type = CourseType.Teacher,
            TeacherId = 10,
            ClassCode = "ENG2024",
            MaxStudent = 30
        };

        // Assert
        Assert.Equal(CourseType.Teacher, course.Type);
        Assert.NotNull(course.TeacherId);
        Assert.NotNull(course.ClassCode);
    }

    [Fact]
    public void Course_SystemCourse_HasNoTeacherOrClassCode()
    {
        // Arrange & Act
        var course = new Course
        {
            Type = CourseType.System,
            TeacherId = null,
            ClassCode = null
        };

        // Assert
        Assert.Equal(CourseType.System, course.Type);
        Assert.Null(course.TeacherId);
        Assert.Null(course.ClassCode);
    }

    [Fact]
    public void Course_IsFeatured_CanBeToggledOnOff()
    {
        // Arrange
        var course = new Course { IsFeatured = false };
        Assert.False(course.IsFeatured);

        // Act - Bật Featured
        course.IsFeatured = true;
        Assert.True(course.IsFeatured);

        // Act - Tắt Featured
        course.IsFeatured = false;
        Assert.False(course.IsFeatured);
    }

    [Fact]
    public void Course_ImageProperties_CanBeSetAndCleared()
    {
        // Arrange
        var course = new Course
        {
            ImageKey = "image123",
            ImageType = "image/png"
        };

        // Assert - Có ảnh
        Assert.NotNull(course.ImageKey);
        Assert.NotNull(course.ImageType);

        // Act - Xóa ảnh
        course.ImageKey = null;
        course.ImageType = null;

        // Assert - Không còn ảnh
        Assert.Null(course.ImageKey);
        Assert.Null(course.ImageType);
    }
}