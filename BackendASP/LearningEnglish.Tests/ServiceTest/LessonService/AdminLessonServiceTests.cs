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

namespace LearningEnglish.Tests.ServiceTest.LessonService;

public class AdminLessonServiceTests
{
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ILogger<AdminLessonService>> _loggerMock;
    private readonly Mock<ILessonImageService> _lessonImageServiceMock;
    private readonly AdminLessonService _service;

    public AdminLessonServiceTests()
    {
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _mapperMock = new Mock<IMapper>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _loggerMock = new Mock<ILogger<AdminLessonService>>();
        _lessonImageServiceMock = new Mock<ILessonImageService>();

        _service = new AdminLessonService(
            _lessonRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _courseRepositoryMock.Object,
            _lessonImageServiceMock.Object
        );
    }

    [Fact]
    public async Task AdminAddLesson_CourseNotFound_ReturnsNotFound()
    {
        // Arrange
        var dto = new AdminCreateLessonDto { CourseId = 1 };
        _courseRepositoryMock.Setup(r => r.GetCourseById(1)).ReturnsAsync((Course)null);

        // Act
        var result = await _service.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy khóa học hoặc bạn không có quyền truy cập", result.Message);
    }

    [Fact]
    public async Task AdminAddLesson_NotSystemCourse_ReturnsForbidden()
    {
        // Arrange
        var dto = new AdminCreateLessonDto { CourseId = 1 };
        var course = new Course { CourseId = 1, Type = CourseType.Teacher };
        _courseRepositoryMock.Setup(r => r.GetCourseById(1)).ReturnsAsync(course);

        // Act
        var result = await _service.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Chỉ admin mới có thể thêm bài học", result.Message);
    }

    [Fact]
    public async Task AdminAddLesson_DuplicateTitle_ReturnsBadRequest()
    {
        // Arrange
        var dto = new AdminCreateLessonDto { CourseId = 1, Title = "Intro" };
        var course = new Course { CourseId = 1, Type = CourseType.System };
        _courseRepositoryMock.Setup(r => r.GetCourseById(1)).ReturnsAsync(course);
        _lessonRepositoryMock.Setup(r => r.LessonIncourse("Intro", 1)).ReturnsAsync(true);

        // Act
        var result = await _service.AdminAddLesson(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Bài học đã tồn tại trong khóa học này", result.Message);
    }

    [Fact]
    public async Task AdminAddLesson_Success_ReturnsCreatedLesson()
    {
        // Arrange
        var dto = new AdminCreateLessonDto { CourseId = 1, Title = "Intro", ImageTempKey = "temp" };
        var course = new Course { CourseId = 1, Type = CourseType.System };
        
        _courseRepositoryMock.Setup(r => r.GetCourseById(1)).ReturnsAsync(course);
        _lessonRepositoryMock.Setup(r => r.LessonIncourse("Intro", 1)).ReturnsAsync(false);
        _lessonImageServiceMock.Setup(s => s.CommitImageAsync("temp")).ReturnsAsync("real-key");
        _lessonImageServiceMock.Setup(s => s.BuildImageUrl("real-key")).Returns("http://img.com");
        
        _mapperMock.Setup(m => m.Map<LessonDto>(It.IsAny<Lesson>()))
            .Returns(new LessonDto { Title = "Intro" });

        // Act
        var result = await _service.AdminAddLesson(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _lessonRepositoryMock.Verify(r => r.AddLesson(It.IsAny<Lesson>()), Times.Once);
    }

    [Fact]
    public async Task DeleteLesson_Success_DeletesLessonAndImage()
    {
        // Arrange
        var lessonId = 1;
        var lesson = new Lesson { LessonId = 1, ImageKey = "key" };
        _lessonRepositoryMock.Setup(r => r.GetLessonById(lessonId)).ReturnsAsync(lesson);

        // Act
        var result = await _service.DeleteLesson(lessonId);

        // Assert
        Assert.True(result.Success);
        _lessonImageServiceMock.Verify(s => s.DeleteImageAsync("key"), Times.Once);
        _lessonRepositoryMock.Verify(r => r.DeleteLesson(lessonId), Times.Once);
    }
}
