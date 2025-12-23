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
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<UserEnrollmentService>> _loggerMock;
    private readonly UserEnrollmentService _userEnrollmentService;

    public UserEnrollmentServiceTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _paymentRepositoryMock = new Mock<IPaymentRepository>();
        _teacherPackageRepositoryMock = new Mock<ITeacherPackageRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<UserEnrollmentService>>();

        _userEnrollmentService = new UserEnrollmentService(
            _courseRepositoryMock.Object,
            _paymentRepositoryMock.Object,
            _teacherPackageRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _userRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object
        );
    }

    #region EnrollInCourseAsync Tests

    [Fact]
    public async Task EnrollInCourseAsync_WithFreeCourse_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new EnrollCourseDto { CourseId = 1 };

        var course = new Course
        {
            CourseId = 1,
            Title = "Free Course",
            Price = 0,
            Type = CourseType.System,
            EnrollmentCount = 5,
            MaxStudent = 100
        };

        var user = new User
        {
            UserId = userId,
            Email = "test@example.com"
        };
        // FullName is read-only, calculated from FirstName and LastName

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.CourseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(dto.CourseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, dto.CourseId))
            .Returns(Task.CompletedTask);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        _notificationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(x => x.SendNotifyJoinCourseAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Đăng ký khóa học thành công", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, dto.CourseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var dto = new EnrollCourseDto { CourseId = 999 };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.CourseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy khóa học", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithAlreadyEnrolled_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var dto = new EnrollCourseDto { CourseId = 1 };

        var course = new Course
        {
            CourseId = 1,
            Title = "Course",
            Price = 0
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.CourseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(dto.CourseId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đăng ký khóa học này rồi", result.Message);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithPaidCourseWithoutPayment_ReturnsPaymentRequired()
    {
        // Arrange
        var userId = 1;
        var dto = new EnrollCourseDto { CourseId = 1 };

        var course = new Course
        {
            CourseId = 1,
            Title = "Paid Course",
            Price = 100000
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.CourseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(dto.CourseId, userId))
            .ReturnsAsync(false);

        _paymentRepositoryMock
            .Setup(x => x.GetSuccessfulPaymentByUserAndCourseAsync(userId, dto.CourseId))
            .ReturnsAsync((Payment?)null);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(402, result.StatusCode);
        Assert.Contains("Hãy thanh toán khóa học trước khi đăng ký", result.Message);
    }

    [Fact]
    public async Task EnrollInCourseAsync_WithFullCourse_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var dto = new EnrollCourseDto { CourseId = 1 };

        var course = new Course
        {
            CourseId = 1,
            Title = "Full Course",
            Price = 0,
            EnrollmentCount = 100,
            MaxStudent = 100
        };

        _courseRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.CourseId))
            .ReturnsAsync(course);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(dto.CourseId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đầy", result.Message);
    }

    #endregion

    #region UnenrollFromCourseAsync Tests

    [Fact]
    public async Task UnenrollFromCourseAsync_WithEnrolledCourse_ReturnsSuccess()
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
        var result = await _userEnrollmentService.UnenrollFromCourseAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Hủy đăng ký khóa học thành công", result.Message);

        _courseRepositoryMock.Verify(x => x.UnenrollUserFromCourse(courseId, userId), Times.Once);
    }

    [Fact]
    public async Task UnenrollFromCourseAsync_WithNotEnrolledCourse_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var courseId = 1;

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userEnrollmentService.UnenrollFromCourseAsync(courseId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("chưa đăng ký khóa học này", result.Message);

        _courseRepositoryMock.Verify(x => x.UnenrollUserFromCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region EnrollInCourseByClassCodeAsync Tests

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithValidClassCode_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";

        var course = new Course
        {
            CourseId = 1,
            Title = "Course",
            ClassCode = classCode,
            Price = 0,
            EnrollmentCount = 5,
            MaxStudent = 100,
            Type = CourseType.System
        };

        var courses = new List<Course> { course };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(courses);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(course.CourseId, userId))
            .ReturnsAsync(false);

        _courseRepositoryMock
            .Setup(x => x.EnrollUserInCourse(userId, course.CourseId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Đăng ký khóa học thành công qua mã lớp học", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(userId, course.CourseId), Times.Once);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithInvalidClassCode_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var classCode = "INVALID";

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(new List<Course>());

        // Act
        var result = await _userEnrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy khóa học với mã lớp học này", result.Message);

        _courseRepositoryMock.Verify(x => x.EnrollUserInCourse(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task EnrollInCourseByClassCodeAsync_WithAlreadyEnrolled_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var classCode = "ABC123";

        var course = new Course
        {
            CourseId = 1,
            Title = "Course",
            ClassCode = classCode,
            Price = 0
        };

        var courses = new List<Course> { course };

        _courseRepositoryMock
            .Setup(x => x.SearchCoursesByClassCode(classCode))
            .ReturnsAsync(courses);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(course.CourseId, userId))
            .ReturnsAsync(true);

        // Act
        var result = await _userEnrollmentService.EnrollInCourseByClassCodeAsync(classCode, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã đăng ký khóa học này rồi", result.Message);
    }

    #endregion
}
