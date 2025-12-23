using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application.CourseServices;

public class EnrollmentQueryServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ICourseProgressRepository> _courseProgressRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<EnrollmentQueryService>> _loggerMock;
    private readonly EnrollmentQueryService _enrollmentQueryService;

    public EnrollmentQueryServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _courseRepositoryMock = new Mock<ICourseRepository>();
        _courseProgressRepositoryMock = new Mock<ICourseProgressRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<EnrollmentQueryService>>();

        _enrollmentQueryService = new EnrollmentQueryService(
            _courseRepositoryMock.Object,
            _courseProgressRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    #region GetMyEnrolledCoursesAsync Tests

    [Fact]
    public async Task GetMyEnrolledCoursesAsync_WithEnrolledCourses_ReturnsCoursesWithProgress()
    {
        // Arrange
        var userId = 1;
        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                ImageKey = "real/course1.jpg",
                Lessons = new List<Lesson>()
            },
            new Course
            {
                CourseId = 2,
                Title = "Course 2",
                ImageKey = "real/course2.jpg",
                Lessons = new List<Lesson>()
            }
        };

        // CourseProgress.IsCompleted is read-only, calculated property
        // We'll just set the values that can be set
        var courseProgress1 = new CourseProgress
        {
            UserId = userId,
            CourseId = 1,
            ProgressPercentage = 50,
            CompletedLessons = 2,
            TotalLessons = 4
        };

        var courseDtos = new List<EnrolledCourseWithProgressDto>
        {
            new EnrolledCourseWithProgressDto
            {
                CourseId = 1,
                Title = "Course 1"
            },
            new EnrolledCourseWithProgressDto
            {
                CourseId = 2,
                Title = "Course 2"
            }
        };

        _courseRepositoryMock
            .Setup(x => x.GetEnrolledCoursesByUser(userId))
            .ReturnsAsync(courses);

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, 1))
            .ReturnsAsync(courseProgress1);

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, 2))
            .ReturnsAsync((CourseProgress?)null);

        _mapperMock
            .Setup(x => x.Map<EnrolledCourseWithProgressDto>(It.IsAny<Course>()))
            .Returns((Course c) => courseDtos.First(d => d.CourseId == c.CourseId));

        // Act
        var result = await _enrollmentQueryService.GetMyEnrolledCoursesAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());

        _courseRepositoryMock.Verify(x => x.GetEnrolledCoursesByUser(userId), Times.Once);
    }

    [Fact]
    public async Task GetMyEnrolledCoursesAsync_WithNoEnrolledCourses_ReturnsEmptyList()
    {
        // Arrange
        var userId = 1;

        _courseRepositoryMock
            .Setup(x => x.GetEnrolledCoursesByUser(userId))
            .ReturnsAsync(new List<Course>());

        // Act
        var result = await _enrollmentQueryService.GetMyEnrolledCoursesAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
        Assert.Contains("No enrolled courses found", result.Message);
    }

    #endregion

    #region GetMyEnrolledCoursesPagedAsync Tests

    [Fact]
    public async Task GetMyEnrolledCoursesPagedAsync_WithEnrolledCourses_ReturnsPagedCourses()
    {
        // Arrange
        var userId = 1;
        var pageRequest = new PageRequest { PageNumber = 1, PageSize = 10 };

        var courses = new List<Course>
        {
            new Course
            {
                CourseId = 1,
                Title = "Course 1",
                ImageKey = "real/course1.jpg"
            }
        };

        var pagedCourses = new PagedResult<Course>
        {
            Items = courses,
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 10
        };

        var courseDto = new EnrolledCourseWithProgressDto
        {
            CourseId = 1,
            Title = "Course 1"
        };

        _courseRepositoryMock
            .Setup(x => x.GetEnrolledCoursesByUserPagedAsync(userId, pageRequest))
            .ReturnsAsync(pagedCourses);

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, 1))
            .ReturnsAsync((CourseProgress?)null);

        _mapperMock
            .Setup(x => x.Map<EnrolledCourseWithProgressDto>(It.IsAny<Course>()))
            .Returns(courseDto);

        // Act
        var result = await _enrollmentQueryService.GetMyEnrolledCoursesPagedAsync(userId, pageRequest);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal(1, result.Data.TotalCount);

        _courseRepositoryMock.Verify(x => x.GetEnrolledCoursesByUserPagedAsync(userId, pageRequest), Times.Once);
    }

    [Fact]
    public async Task GetMyEnrolledCoursesPagedAsync_WithNoEnrolledCourses_ReturnsEmptyPagedResult()
    {
        // Arrange
        var userId = 1;
        var pageRequest = new PageRequest { PageNumber = 1, PageSize = 10 };

        var pagedCourses = new PagedResult<Course>
        {
            Items = new List<Course>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 10
        };

        _courseRepositoryMock
            .Setup(x => x.GetEnrolledCoursesByUserPagedAsync(userId, pageRequest))
            .ReturnsAsync(pagedCourses);

        // Act
        var result = await _enrollmentQueryService.GetMyEnrolledCoursesPagedAsync(userId, pageRequest);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
        Assert.Equal(0, result.Data.TotalCount);
    }

    #endregion
}

