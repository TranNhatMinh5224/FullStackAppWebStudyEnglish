using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Tests.Application.CourseServices;

public class UserEnrollmentServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IPaymentRepository> _paymentRepositoryMock;
    private readonly Mock<ITeacherPackageRepository> _teacherPackageRepositoryMock;
    private readonly Mock<ILogger<UserEnrollmentService>> _loggerMock;
    private readonly UserEnrollmentService _enrollmentService;

    public UserEnrollmentServiceTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _teacherPackageRepositoryMock = new Mock<ITeacherPackageRepository>();
        _loggerMock = new Mock<ILogger<UserEnrollmentService>>();

        _enrollmentService = new UserEnrollmentService(
            _courseRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _teacherPackageRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region EnrollInCourseAsync Tests

    [Fact]
    public async Task EnrollInCourseAsync_WithFreeCourse_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Free Course",
            Price = 0, // Free course
            Status = CourseStatus.Published,
            Type = CourseType.System,
            MaxStudent = 0 // No limit
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("thành công", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, courseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithPaidCourseWithoutPayment_ReturnsPaymentRequired()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Paid Course",
            Price = 100000,
            Status = CourseStatus.Published
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _paymentRepositoryMock
            .Setup(x => x.GetSuccessfulPaymentByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(402, result.StatusCode); // Payment Required
        Assert.Contains("thanh toán", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithPaidCourseWithPayment_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Paid Course",
            Price = 100000,
            Status = CourseStatus.Published,
            MaxStudent = 0
        };

        var payment = new Payment
        {
            PaymentId = 1,
            UserId = userId,
            ProductId = courseId,
            ProductType = ProductType.Course,
            Status = PaymentStatus.Completed,
            Amount = 100000
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _paymentRepositoryMock
            .Setup(x => x.GetSuccessfulPaymentByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync(payment);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, courseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithAlreadyEnrolled_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Course",
            Price = 0
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true); // Already enrolled

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đăng ký", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithFullCourse_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Full Course",
            Price = 0,
            MaxStudent = 10,
            EnrollmentCount = 10 // Full
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đầy", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var courseId = 999;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithTeacherCourseNoPackage_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var teacherId = 2;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Teacher Course",
            Price = 0,
            Type = CourseType.Teacher,
            TeacherId = teacherId,
            MaxStudent = 0
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync((TeacherPackage?)null); // No active package

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("không nhận học viên mới", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithTeacherCourseWithPackage_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var teacherId = 2;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Teacher Course",
            Price = 0,
            Type = CourseType.Teacher,
            TeacherId = teacherId,
            MaxStudent = 0
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            PackageName = "Basic Package",
            Level = PackageLevel.Basic,
            Price = 100000
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, courseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithSystemCourse_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "System Course",
            Price = 0,
            Type = CourseType.System,
            MaxStudent = 0
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);

        // System course không cần check teacher package
        _teacherPackageRepositoryMock.Verify(x => x.GetInformationTeacherpackage(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithNullPrice_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;
        var enrollDto = new EnrollCourseDto { CourseId = courseId };

        var course = new Course
        {
            CourseId = courseId,
            Title = "Free Course",
            Price = null, // Null price = free
            MaxStudent = 0
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseAsync(enrollDto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        // Không cần check payment cho course miễn phí
        _paymentRepositoryMock.Verify(x => x.GetSuccessfulPaymentByUserAndCourseAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region UnenrollFromCourseAsync Tests

    [Fact]
    public async Task UnenrollFromCourseAsync_WithValidEnrollment_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _courseRepositoryMock
            .Setup(x => x.UnenrollUserFromCourse(courseId, userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.UnenrollFromCourseAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("thành công", result.Message);

        _courseRepositoryMock.Verify(x => x.UnenrollUserFromCourse(courseId, userId), Times.Once);
    }

    [Fact]
    public async Task UnenrollFromCourseAsync_WithNotEnrolled_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _enrollmentService.UnenrollFromCourseAsync(courseId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("chưa đăng ký", result.Message);

        _courseRepositoryMock.Verify(x => x.UnenrollUserFromCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UnenrollFromCourseAsync_WhenExceptionThrown_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _courseRepositoryMock
            .Setup(x => x.UnenrollUserFromCourse(courseId, userId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _enrollmentService.UnenrollFromCourseAsync(courseId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi", result.Message);
    }

    #endregion

    #region EnrollInCourseByClassCodeAsync Tests

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithValidCode_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Course with Class Code",
            Price = 0,
            ClassCode = classCode,
            MaxStudent = 0
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course> { course });

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("mã lớp học", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, courseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithInvalidCode_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var classCode = "INVALID123";

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course>()); // Empty list

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithFullCourse_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Full Course",
            ClassCode = classCode,
            MaxStudent = 10,
            EnrollmentCount = 10 // Full
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course> { course });

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đầy", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithAlreadyEnrolled_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Course",
            ClassCode = classCode,
            MaxStudent = 0
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course> { course });

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true); // Already enrolled

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đăng ký", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithTeacherCourseNoPackage_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var teacherId = 2;
        var classCode = "ABC123";
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Teacher Course",
            ClassCode = classCode,
            Type = CourseType.Teacher,
            TeacherId = teacherId,
            Price = 0,
            MaxStudent = 0
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course> { course });

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync((TeacherPackage?)null);

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("không nhận học viên mới", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithTeacherCourseWithPackage_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var teacherId = 2;
        var classCode = "ABC123";
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Teacher Course",
            ClassCode = classCode,
            Type = CourseType.Teacher,
            TeacherId = teacherId,
            Price = 0,
            MaxStudent = 0
        };

        var teacherPackage = new TeacherPackage
        {
            TeacherPackageId = 1,
            PackageName = "Basic Package",
            Level = PackageLevel.Basic
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course> { course });

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _teacherPackageRepositoryMock
            .Setup(x => x.GetInformationTeacherpackage(teacherId))
            .ReturnsAsync(teacherPackage);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, courseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WhenCapacityReached_HandlesException()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Course",
            ClassCode = classCode,
            MaxStudent = 10,
            EnrollmentCount = 9 // One slot left
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course> { course });

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, courseId))
            .ThrowsAsync(new InvalidOperationException("Cannot enroll more students, maximum capacity reached."));

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đầy học viên", result.Message);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WhenExceptionThrown_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _enrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi", result.Message);
    }

    #endregion
}

