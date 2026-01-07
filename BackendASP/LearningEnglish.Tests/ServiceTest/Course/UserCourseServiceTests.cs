using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.Courses;

public class UserCourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ICourseProgressRepository> _courseProgressRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UserCourseService>> _loggerMock;
    private readonly Mock<ICourseImageService> _courseImageServiceMock;
    private readonly UserCourseService _service;

    public UserCourseServiceTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _courseProgressRepositoryMock = new Mock<ICourseProgressRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserCourseService>>();
        _courseImageServiceMock = new Mock<ICourseImageService>();

        _service = new UserCourseService(
            _courseRepositoryMock.Object,
            _courseProgressRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _courseImageServiceMock.Object
        );
    }

    [Fact]
    public async Task GetSystemCoursesAsync_Success_ReturnsCourses()
    {
        // Arrange
        var courses = new List<LearningEnglish.Domain.Entities.Course> { new LearningEnglish.Domain.Entities.Course { CourseId = 1, Title = "Course 1", ImageKey = "image1.jpg" } };
        var courseDtos = new List<SystemCoursesListResponseDto> 
        { 
            new SystemCoursesListResponseDto { CourseId = 1, Title = "Course 1", ImageUrl = "image1.jpg" } 
        };

        _courseRepositoryMock.Setup(r => r.GetSystemCourses()).ReturnsAsync(courses);
        _mapperMock.Setup(m => m.Map<IEnumerable<SystemCoursesListResponseDto>>(courses)).Returns(courseDtos);
        _courseImageServiceMock.Setup(s => s.BuildImageUrl("image1.jpg")).Returns("https://example.com/image1.jpg");

        // Act
        var result = await _service.GetSystemCoursesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Equal("https://example.com/image1.jpg", result.Data.First().ImageUrl);
        Assert.False(result.Data.First().IsEnrolled);
    }

    [Fact]
    public async Task GetSystemCoursesAsync_WithUserId_ChecksEnrollment()
    {
        // Arrange
        var userId = 10;
        var courses = new List<LearningEnglish.Domain.Entities.Course> { new LearningEnglish.Domain.Entities.Course { CourseId = 1, Title = "Course 1" } };
        var courseDtos = new List<SystemCoursesListResponseDto> 
        { 
            new SystemCoursesListResponseDto { CourseId = 1, Title = "Course 1" } 
        };

        _courseRepositoryMock.Setup(r => r.GetSystemCourses()).ReturnsAsync(courses);
        _mapperMock.Setup(m => m.Map<IEnumerable<SystemCoursesListResponseDto>>(courses)).Returns(courseDtos);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(1, userId)).ReturnsAsync(true);

        // Act
        var result = await _service.GetSystemCoursesAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data.First().IsEnrolled);
    }

    [Fact]
    public async Task GetCourseByIdAsync_CourseExists_ReturnsCourseDetail()
    {
        // Arrange
        var courseId = 1;
        var course = new LearningEnglish.Domain.Entities.Course { CourseId = courseId, Title = "Course 1", ImageKey = "image1.jpg" };
        var courseDto = new CourseDetailWithEnrollmentDto { CourseId = courseId, Title = "Course 1", ImageUrl = "image1.jpg" };

        _courseRepositoryMock.Setup(r => r.GetCourseById(courseId)).ReturnsAsync(course);
        _mapperMock.Setup(m => m.Map<CourseDetailWithEnrollmentDto>(course)).Returns(courseDto);
        _courseImageServiceMock.Setup(s => s.BuildImageUrl("image1.jpg")).Returns("https://example.com/image1.jpg");

        // Act
        var result = await _service.GetCourseByIdAsync(courseId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("https://example.com/image1.jpg", result.Data.ImageUrl);
    }

    [Fact]
    public async Task GetCourseByIdAsync_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var courseId = 1;
        _courseRepositoryMock.Setup(r => r.GetCourseById(courseId)).ReturnsAsync((LearningEnglish.Domain.Entities.Course)null);

        // Act
        var result = await _service.GetCourseByIdAsync(courseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task SearchCoursesAsync_Success_ReturnsResults()
    {
        // Arrange
        var keyword = "test";
        var courses = new List<LearningEnglish.Domain.Entities.Course> { new LearningEnglish.Domain.Entities.Course { CourseId = 1, Title = "Test Course" } };
        var courseDtos = new List<SystemCoursesListResponseDto> 
        { 
            new SystemCoursesListResponseDto { CourseId = 1, Title = "Test Course" } 
        };

        _courseRepositoryMock.Setup(r => r.SearchCourses(keyword)).ReturnsAsync(courses);
        _mapperMock.Setup(m => m.Map<IEnumerable<SystemCoursesListResponseDto>>(courses)).Returns(courseDtos);

        // Act
        var result = await _service.SearchCoursesAsync(keyword);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task GetMyEnrolledCoursesPagedAsync_ReturnsPagedResults()
    {
        // Arrange
        var userId = 1;
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };
        var courses = new List<LearningEnglish.Domain.Entities.Course> { new LearningEnglish.Domain.Entities.Course { CourseId = 1, Title = "Enrolled Course" } };
        var pagedResult = new PagedResult<LearningEnglish.Domain.Entities.Course> 
        { 
            Items = courses, 
            TotalCount = 1, 
            PageNumber = 1, 
            PageSize = 10 
        };
        var courseDto = new EnrolledCourseWithProgressDto { CourseId = 1, Title = "Enrolled Course" };
        var progress = new CourseProgress { CourseId = 1, UserId = userId, ProgressPercentage = 50 };

        _courseRepositoryMock.Setup(r => r.GetEnrolledCoursesByUserPagedAsync(userId, request)).ReturnsAsync(pagedResult);
        _mapperMock.Setup(m => m.Map<EnrolledCourseWithProgressDto>(It.IsAny<LearningEnglish.Domain.Entities.Course>())).Returns(courseDto);
        _courseProgressRepositoryMock.Setup(r => r.GetByUserAndCourseAsync(userId, 1)).ReturnsAsync(progress);

        // Act
        var result = await _service.GetMyEnrolledCoursesPagedAsync(userId, request);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data.Items);
        Assert.Equal(50, result.Data.Items.First().ProgressPercentage);
    }
}
