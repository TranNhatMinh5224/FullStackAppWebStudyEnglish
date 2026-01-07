using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Tests.ServiceTest.CourseServices;

public class AdminCourseServiceTests
{
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AdminCourseService>> _loggerMock;
    private readonly Mock<ICourseImageService> _courseImageServiceMock;
    private readonly AdminCourseService _service;

    public AdminCourseServiceTests()
    {
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AdminCourseService>>();
        _courseImageServiceMock = new Mock<ICourseImageService>();

        _service = new AdminCourseService(
            _courseRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _courseImageServiceMock.Object
        );
    }

    [Fact]
    public async Task GetCourseTypesAsync_Success_ReturnsCourseTypes()
    {
        // Arrange
        var courseTypes = new List<CourseTypeDto> { new CourseTypeDto { Id = 1, Name = "System" } };
        _courseRepositoryMock.Setup(r => r.GetCourseTypesAsync()).ReturnsAsync(courseTypes);

        // Act
        var result = await _service.GetCourseTypesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal(courseTypes, result.Data);
    }

    [Fact]
    public async Task AdminCreateCourseAsync_Success_ReturnsCreatedCourse()
    {
        // Arrange
        var request = new AdminCreateCourseRequestDto 
        { 
            Title = "New Course", 
            Description = "Desc", 
            Type = CourseType.System,
            ImageTempKey = "temp-key",
            ImageType = "image/jpeg"
        };
        
        var committedKey = "real-key";
        _courseImageServiceMock.Setup(s => s.CommitImageAsync(request.ImageTempKey)).ReturnsAsync(committedKey);
        _courseImageServiceMock.Setup(s => s.BuildImageUrl(committedKey)).Returns("http://url.com/image.jpg");
        
        _mapperMock.Setup(m => m.Map<CourseResponseDto>(It.IsAny<Course>()))
            .Returns(new CourseResponseDto { Title = request.Title });

        // Act
        var result = await _service.AdminCreateCourseAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _courseRepositoryMock.Verify(r => r.AddCourse(It.Is<Course>(c => c.ImageKey == committedKey)), Times.Once);
    }

    [Fact]
    public async Task AdminUpdateCourseAsync_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        _courseRepositoryMock.Setup(r => r.GetCourseById(It.IsAny<int>())).ReturnsAsync((Course)null);

        // Act
        var result = await _service.AdminUpdateCourseAsync(1, new AdminUpdateCourseRequestDto());

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy khóa học", result.Message);
    }

    [Fact]
    public async Task DeleteCourseAsync_Success_DeletesCourseAndImage()
    {
        // Arrange
        var courseId = 1;
        var course = new Course { CourseId = courseId, ImageKey = "old-key" };
        _courseRepositoryMock.Setup(r => r.GetCourseById(courseId)).ReturnsAsync(course);

        // Act
        var result = await _service.DeleteCourseAsync(courseId);

        // Assert
        Assert.True(result.Success);
        _courseImageServiceMock.Verify(s => s.DeleteImageAsync("old-key"), Times.Once);
        _courseRepositoryMock.Verify(r => r.DeleteCourse(courseId), Times.Once);
    }

    [Fact]
    public async Task GetAllCoursesPagedAsync_Success_ReturnsPagedData()
    {
        // Arrange
        var parameters = new AdminCourseQueryParameters();
        var courses = new List<Course> { new Course { CourseId = 1, Title = "C1", ImageKey = "k1" } };
        var pagedResult = new PagedResult<Course> { Items = courses, TotalCount = 1 };
        
        _courseRepositoryMock.Setup(r => r.GetAllCoursesPagedForAdminAsync(parameters)).ReturnsAsync(pagedResult);
        _mapperMock.Setup(m => m.Map<AdminCourseListResponseDto>(It.IsAny<Course>()))
            .Returns(new AdminCourseListResponseDto { CourseId = 1 });
        _courseImageServiceMock.Setup(s => s.BuildImageUrl("k1")).Returns("http://url.com/k1.jpg");

        // Act
        var result = await _service.GetAllCoursesPagedAsync(parameters);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Single(result.Data.Items);
        Assert.Equal("http://url.com/k1.jpg", result.Data.Items.First().ImageUrl);
    }
}
