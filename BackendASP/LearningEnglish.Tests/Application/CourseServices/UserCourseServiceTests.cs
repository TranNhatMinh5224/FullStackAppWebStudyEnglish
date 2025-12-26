using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using LearningEnglish.Application.Mappings;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace LearningEnglish.Tests.Application.CourseServices;

public class UserCourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ICourseProgressRepository> _courseProgressRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UserCourseService>> _loggerMock;
    private readonly UserCourseService _userCourseService;

    public UserCourseServiceTests()
    {
        // Cau hinh BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _courseRepositoryMock = new Mock<ICourseRepository>();
        _courseProgressRepositoryMock = new Mock<ICourseProgressRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserCourseService>>();

        _userCourseService = new UserCourseService(
            _courseRepositoryMock.Object,
            _courseProgressRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    #region GetSystemCoursesAsync Tests

    [Fact]
    public async Task GetSystemCoursesAsync_WithNoUserId_ReturnsSystemCourses()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                Type = CourseType.System,
                Status = CourseStatus.Published,
                Price = 0,
                ImageKey = "course1.jpg"
            },
            new Course
            {
                CourseId = 2,
                Title = "Course 2",
                Type = CourseType.System,
                Status = CourseStatus.Published,
                Price = 100000,
                ImageKey = "course2.jpg"
            }
        };

        var courseDtos = new List<SystemCoursesListResponseDto>
        {
            new SystemCoursesListResponseDto
            {
                CourseId = 1,
                Title = "Course 1",
                ImageUrl = "course1.jpg",
                IsEnrolled = false
            },
            new SystemCoursesListResponseDto
            {
                CourseId = 2,
                Title = "Course 2",
                ImageUrl = "course2.jpg",
                IsEnrolled = false
            }
        };

        _courseRepositoryMock
            .Setup(x => x.GetSystemCourses())
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(courseDtos);

        // Act
        var result = await _userCourseService.GetSystemCoursesAsync(null);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.All(result.Data, dto => Assert.False(dto.IsEnrolled)); // No userId, all should be false

        _courseRepositoryMock.Verify(x => x.IsUserEnrolled(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetSystemCoursesAsync_WithUserId_ReturnsSystemCoursesWithEnrollmentStatus()
    {
        // Arrange
        var userId = 1;
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                Type = CourseType.System,
                Status = CourseStatus.Published,
                ImageKey = "course1.jpg"
            },
            new Course
            {
                CourseId = 2,
                Title = "Course 2",
                Type = CourseType.System,
                Status = CourseStatus.Published,
                ImageKey = "course2.jpg"
            }
        };

        var courseDtos = new List<SystemCoursesListResponseDto>
        {
            new SystemCoursesListResponseDto
            {
                CourseId = 1,
                Title = "Course 1",
                ImageUrl = "course1.jpg"
            },
            new SystemCoursesListResponseDto
            {
                CourseId = 2,
                Title = "Course 2",
                ImageUrl = "course2.jpg"
            }
        };

        _courseRepositoryMock
            .Setup(x => x.GetSystemCourses())
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(courseDtos);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(1, userId))
            .ReturnsAsync(true); // User enrolled in course 1

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(2, userId))
            .ReturnsAsync(false); // User not enrolled in course 2

        // Act
        var result = await _userCourseService.GetSystemCoursesAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());

        var enrolledCourse = result.Data.First(dto => dto.CourseId == 1);
        var notEnrolledCourse = result.Data.First(dto => dto.CourseId == 2);

        Assert.True(enrolledCourse.IsEnrolled);
        Assert.False(notEnrolledCourse.IsEnrolled);

        _courseRepositoryMock.Verify(x => x.IsUserEnrolled(1, userId), Times.Once);
        _courseRepositoryMock.Verify(x => x.IsUserEnrolled(2, userId), Times.Once);
    }

    [Fact]
    public async Task GetSystemCoursesAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var courses = new List<Course>();

        _courseRepositoryMock
            .Setup(x => x.GetSystemCourses())
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(new List<SystemCoursesListResponseDto>());

        // Act
        var result = await _userCourseService.GetSystemCoursesAsync(null);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetSystemCoursesAsync_WithException_ReturnsError()
    {
        // Arrange
        _courseRepositoryMock
            .Setup(x => x.GetSystemCourses())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userCourseService.GetSystemCoursesAsync(null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi", result.Message);
    }

    [Fact]
    public async Task GetSystemCoursesAsync_WithNullImageKey_DoesNotBuildUrl()
    {
        // Arrange
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                Type = CourseType.System,
                ImageKey = null // No image
            }
        };

        var courseDtos = new List<SystemCoursesListResponseDto>
        {
            new SystemCoursesListResponseDto
            {
                CourseId = 1,
                Title = "Course 1",
                ImageUrl = null
            }
        };

        _courseRepositoryMock
            .Setup(x => x.GetSystemCourses())
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(courseDtos);

        // Act
        var result = await _userCourseService.GetSystemCoursesAsync(null);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.First().ImageUrl);
    }

    #endregion

    #region GetCourseByIdAsync Tests

    [Fact]
    public async Task GetCourseByIdAsync_WithValidCourseId_ReturnsCourseDetail()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            DescriptionMarkdown = "Test Description",
            Type = CourseType.System,
            Status = CourseStatus.Published,
            ImageKey = "course1.jpg",
            Price = 0
        };

        var courseDto = new CourseDetailWithEnrollmentDto
        {
            CourseId = courseId,
            Title = "Test Course",
            Description = "Test Description",
            ImageUrl = "course1.jpg"
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<CourseDetailWithEnrollmentDto>(course))
            .Returns(courseDto);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(false);

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(courseId, result.Data.CourseId);
        Assert.False(result.Data.IsEnrolled);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithNonExistentCourse_ReturnsNotFound()
    {
        // Arrange
        var courseId = 999;
        var userId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync((Course?)null);

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithEnrolledUser_ReturnsCourseWithProgress()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System,
            ImageKey = "course1.jpg"
        };

        var courseDto = new CourseDetailWithEnrollmentDto
        {
            CourseId = courseId,
            Title = "Test Course",
            ImageUrl = "course1.jpg"
        };

        var courseProgress = new CourseProgress
        {
            CourseProgressId = 1,
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow.AddDays(-30)
        };
        courseProgress.UpdateProgress(10, 5); // Total: 10, Completed: 5 (50%)

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<CourseDetailWithEnrollmentDto>(course))
            .Returns(courseDto);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync(courseProgress);

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEnrolled);
        Assert.Equal(50, result.Data.ProgressPercentage);
        Assert.Equal(5, result.Data.CompletedLessons);
        Assert.False(result.Data.IsCompleted);
        Assert.NotNull(result.Data.EnrolledAt);

        _courseProgressRepositoryMock.Verify(x => x.GetByUserAndCourseAsync(userId, courseId), Times.Once);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithEnrolledUserNoProgress_ReturnsCourseWithoutProgress()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System
        };

        var courseDto = new CourseDetailWithEnrollmentDto
        {
            CourseId = courseId,
            Title = "Test Course"
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<CourseDetailWithEnrollmentDto>(course))
            .Returns(courseDto);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync((CourseProgress?)null); // No progress record

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEnrolled);
        Assert.Equal(0, result.Data.ProgressPercentage);
        Assert.Equal(0, result.Data.CompletedLessons);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithNoUserId_ReturnsCourseWithoutEnrollment()
    {
        // Arrange
        var courseId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System
        };

        var courseDto = new CourseDetailWithEnrollmentDto
        {
            CourseId = courseId,
            Title = "Test Course"
        };

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<CourseDetailWithEnrollmentDto>(course))
            .Returns(courseDto);

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, null);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsEnrolled);

        _courseRepositoryMock.Verify(x => x.IsUserEnrolled(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        _courseProgressRepositoryMock.Verify(x => x.GetByUserAndCourseAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithCompletedCourse_ReturnsCompletedStatus()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        var course = new Course
        {
            CourseId = courseId,
            Title = "Test Course",
            Type = CourseType.System
        };

        var courseDto = new CourseDetailWithEnrollmentDto
        {
            CourseId = courseId,
            Title = "Test Course"
        };

        var courseProgress = new CourseProgress
        {
            CourseProgressId = 1,
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow.AddDays(-60)
        };
        courseProgress.UpdateProgress(10, 10); // Total: 10, Completed: 10 (100% - completed, CompletedAt will be set automatically)

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(course);

        _mapperMock
            .Setup(x => x.Map<CourseDetailWithEnrollmentDto>(course))
            .Returns(courseDto);

        _courseRepositoryMock
            .Setup(x => x.IsUserEnrolled(courseId, userId))
            .ReturnsAsync(true);

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync(courseProgress);

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsEnrolled);
        Assert.True(result.Data.IsCompleted);
        Assert.Equal(100, result.Data.ProgressPercentage);
        Assert.Equal(10, result.Data.CompletedLessons);
        Assert.NotNull(result.Data.CompletedAt);
    }

    [Fact]
    public async Task GetCourseByIdAsync_WithException_ReturnsError()
    {
        // Arrange
        var courseId = 1;
        var userId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi", result.Message);
    }

    #endregion

    #region SearchCoursesAsync Tests

    [Fact]
    public async Task SearchCoursesAsync_WithValidKeyword_ReturnsMatchingCourses()
    {
        // Arrange
        var keyword = "English";
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "English Grammar",
                Type = CourseType.System,
                Status = CourseStatus.Published,
                ImageKey = "grammar.jpg"
            },
            new Course
            {
                CourseId = 2,
                Title = "English Vocabulary",
                Type = CourseType.System,
                Status = CourseStatus.Published,
                ImageKey = "vocab.jpg"
            }
        };

        var courseDtos = new List<SystemCoursesListResponseDto>
        {
            new SystemCoursesListResponseDto
            {
                CourseId = 1,
                Title = "English Grammar",
                ImageUrl = "grammar.jpg"
            },
            new SystemCoursesListResponseDto
            {
                CourseId = 2,
                Title = "English Vocabulary",
                ImageUrl = "vocab.jpg"
            }
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCourses(keyword))
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(courseDtos);

        // Act
        var result = await _userCourseService.SearchCoursesAsync(keyword);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.All(result.Data, dto => Assert.Contains("English", dto.Title, StringComparison.OrdinalIgnoreCase));

        _courseRepositoryMock.Verify(x => x.SearchCourses(keyword), Times.Once);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithNoResults_ReturnsEmptyList()
    {
        // Arrange
        var keyword = "NonExistentCourse";

        _courseRepositoryMock
            .Setup(x => x.SearchCourses(keyword))
            .ReturnsAsync(new List<Course>());

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(It.IsAny<IEnumerable<Course>>()))
            .Returns(new List<SystemCoursesListResponseDto>());

        // Act
        var result = await _userCourseService.SearchCoursesAsync(keyword);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithEmptyKeyword_ReturnsAllCourses()
    {
        // Arrange
        var keyword = "";
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                Type = CourseType.System,
                Status = CourseStatus.Published
            }
        };

        var courseDtos = new List<SystemCoursesListResponseDto>
        {
            new SystemCoursesListResponseDto
            {
                CourseId = 1,
                Title = "Course 1"
            }
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCourses(keyword))
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(courseDtos);

        // Act
        var result = await _userCourseService.SearchCoursesAsync(keyword);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithNullImageKey_DoesNotBuildUrl()
    {
        // Arrange
        var keyword = "Test";
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Test Course",
                Type = CourseType.System,
                ImageKey = null
            }
        };

        var courseDtos = new List<SystemCoursesListResponseDto>
        {
            new SystemCoursesListResponseDto
            {
                CourseId = 1,
                Title = "Test Course",
                ImageUrl = null
            }
        };

        _courseRepositoryMock
            .Setup(x => x.SearchCourses(keyword))
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(courseDtos);

        // Act
        var result = await _userCourseService.SearchCoursesAsync(keyword);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.First().ImageUrl);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithException_ReturnsError()
    {
        // Arrange
        var keyword = "Test";

        _courseRepositoryMock
            .Setup(x => x.SearchCourses(keyword))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _userCourseService.SearchCoursesAsync(keyword);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi", result.Message);
    }

    [Fact]
    public async Task SearchCoursesAsync_WithWhitespaceKeyword_HandlesCorrectly()
    {
        // Arrange
        var keyword = "   ";
        var courses = new List<Course>();

        _courseRepositoryMock
            .Setup(x => x.SearchCourses(keyword))
            .ReturnsAsync(courses);

        _mapperMock
            .Setup(x => x.Map<IEnumerable<SystemCoursesListResponseDto>>(courses))
            .Returns(new List<SystemCoursesListResponseDto>());

        // Act
        var result = await _userCourseService.SearchCoursesAsync(keyword);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    #endregion
}

