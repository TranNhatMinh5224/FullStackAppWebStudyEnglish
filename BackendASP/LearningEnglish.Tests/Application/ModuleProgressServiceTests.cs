using LearningEnglish.Application.Service;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Tests.Application;

public class ModuleProgressServiceTests
{
    private readonly Mock<IModuleCompletionRepository> _moduleCompletionRepositoryMock;
    private readonly Mock<ILessonCompletionRepository> _lessonCompletionRepositoryMock;
    private readonly Mock<ICourseProgressRepository> _courseProgressRepositoryMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ILogger<ModuleProgressService>> _loggerMock;
    private readonly ModuleProgressService _moduleProgressService;

    public ModuleProgressServiceTests()
    {
        _moduleCompletionRepositoryMock = new Mock<IModuleCompletionRepository>();
        _lessonCompletionRepositoryMock = new Mock<ILessonCompletionRepository>();
        _courseProgressRepositoryMock = new Mock<ICourseProgressRepository>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _loggerMock = new Mock<ILogger<ModuleProgressService>>();

        _moduleProgressService = new ModuleProgressService(
            _moduleCompletionRepositoryMock.Object,
            _lessonCompletionRepositoryMock.Object,
            _courseProgressRepositoryMock.Object,
            _moduleRepositoryMock.Object,
            _lessonRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region StartModuleAsync Tests

    [Fact]
    public async Task StartModuleAsync_WithNewModule_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync((ModuleCompletion?)null);

        _moduleCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.Is<ModuleCompletion>(mc =>
                mc.UserId == userId &&
                mc.ModuleId == moduleId &&
                mc.StartedAt != null)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.StartModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Module started successfully", result.Message);

        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ModuleCompletion>()), Times.Once);
    }

    [Fact]
    public async Task StartModuleAsync_WithExistingCompletion_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;

        var existingCompletion = new ModuleCompletion
        {
            ModuleCompletionId = 1,
            UserId = userId,
            ModuleId = moduleId,
            StartedAt = DateTime.UtcNow.AddDays(-1)
        };

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync(existingCompletion);

        // Act
        var result = await _moduleProgressService.StartModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        // Should not add again if already exists
        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ModuleCompletion>()), Times.Never);
    }

    [Fact]
    public async Task StartModuleAsync_WithException_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _moduleProgressService.StartModuleAsync(userId, moduleId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi khi bắt đầu module", result.Message);
    }

    #endregion

    #region CompleteModuleAsync Tests

    [Fact]
    public async Task CompleteModuleAsync_WithValidModule_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;
        var lessonId = 1;
        var courseId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = lessonId,
            ContentType = ModuleType.FlashCard
        };

        var lesson = new Lesson
        {
            LessonId = lessonId,
            Title = "Test Lesson",
            CourseId = courseId
        };

        var modules = new List<Module> { module };
        var moduleCompletions = new List<ModuleCompletion>();

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync((ModuleCompletion?)null);

        _moduleCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.Is<ModuleCompletion>(mc =>
                mc.UserId == userId &&
                mc.ModuleId == moduleId &&
                mc.IsCompleted)))
            .Returns(Task.CompletedTask);

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdAsync(lessonId))
            .ReturnsAsync(modules);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(moduleCompletions);

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ReturnsAsync((LessonCompletion?)null);

        _lessonCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LessonCompletion>()))
            .Returns(Task.CompletedTask);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ReturnsAsync(new List<Lesson> { lesson });

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<LessonCompletion>());

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(new Course { CourseId = courseId });

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync((CourseProgress?)null);

        _courseProgressRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CourseProgress>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.CompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ModuleCompletion>()), Times.Once);
        _lessonCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LessonCompletion>()), Times.Once);
        _courseProgressRepositoryMock.Verify(x => x.AddAsync(It.IsAny<CourseProgress>()), Times.Once);
    }

    [Fact]
    public async Task CompleteModuleAsync_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var moduleId = 999;

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act
        var result = await _moduleProgressService.CompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Module không tồn tại", result.Message);

        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ModuleCompletion>()), Times.Never);
    }

    [Fact]
    public async Task CompleteModuleAsync_WithExistingCompletion_UpdatesCompletion()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;
        var lessonId = 1;
        var courseId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = lessonId
        };

        var existingCompletion = new ModuleCompletion
        {
            ModuleCompletionId = 1,
            UserId = userId,
            ModuleId = moduleId,
            IsCompleted = false,
            StartedAt = DateTime.UtcNow.AddDays(-1)
        };

        var lesson = new Lesson
        {
            LessonId = lessonId,
            CourseId = courseId
        };

        var modules = new List<Module> { module };
        var moduleCompletions = new List<ModuleCompletion> { existingCompletion };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync(existingCompletion);

        _moduleCompletionRepositoryMock
            .Setup(x => x.UpdateAsync(It.Is<ModuleCompletion>(mc => mc.IsCompleted)))
            .Returns(Task.CompletedTask);

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdAsync(lessonId))
            .ReturnsAsync(modules);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(moduleCompletions);

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ReturnsAsync((LessonCompletion?)null);

        _lessonCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LessonCompletion>()))
            .Returns(Task.CompletedTask);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ReturnsAsync(new List<Lesson> { lesson });

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<LessonCompletion>());

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(new Course { CourseId = courseId });

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync((CourseProgress?)null);

        _courseProgressRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CourseProgress>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.CompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);

        _moduleCompletionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<ModuleCompletion>()), Times.Once);
        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<ModuleCompletion>()), Times.Never);
    }

    #endregion

    #region StartAndCompleteModuleAsync Tests

    [Fact]
    public async Task StartAndCompleteModuleAsync_WithFlashCardModule_AutoCompletes()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;
        var lessonId = 1;
        var courseId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "FlashCard Module",
            LessonId = lessonId,
            ContentType = ModuleType.FlashCard
        };

        var lesson = new Lesson
        {
            LessonId = lessonId,
            CourseId = courseId
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync((ModuleCompletion?)null);

        _moduleCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ModuleCompletion>()))
            .Returns(Task.CompletedTask);

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdAsync(lessonId))
            .ReturnsAsync(new List<Module> { module });

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ModuleCompletion>());

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ReturnsAsync((LessonCompletion?)null);

        _lessonCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LessonCompletion>()))
            .Returns(Task.CompletedTask);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ReturnsAsync(new List<Lesson> { lesson });

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<LessonCompletion>());

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(new Course { CourseId = courseId });

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync((CourseProgress?)null);

        _courseProgressRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CourseProgress>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Module started successfully", result.Message);

        // Should call CompleteModuleAsync for FlashCard
        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.Is<ModuleCompletion>(mc => mc.IsCompleted)), Times.Once);
    }

    [Fact]
    public async Task StartAndCompleteModuleAsync_WithQuizModule_OnlyStarts()
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Quiz Module",
            LessonId = 1,
            ContentType = ModuleType.Quiz // Quiz không auto-complete
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync((ModuleCompletion?)null);

        _moduleCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.Is<ModuleCompletion>(mc => !mc.IsCompleted)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        // Should only start, not complete
        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.Is<ModuleCompletion>(mc => !mc.IsCompleted)), Times.Once);
    }

    [Fact]
    public async Task StartAndCompleteModuleAsync_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var moduleId = 999;

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act
        var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Module không tồn tại", result.Message);
    }

    [Theory]
    [InlineData(ModuleType.FlashCard)]
    [InlineData(ModuleType.Lecture)]
    [InlineData(ModuleType.Video)]
    [InlineData(ModuleType.Reading)]
    public async Task StartAndCompleteModuleAsync_WithAutoCompleteTypes_AutoCompletes(ModuleType contentType)
    {
        // Arrange
        var userId = 1;
        var moduleId = 1;
        var lessonId = 1;
        var courseId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = lessonId,
            ContentType = contentType
        };

        var lesson = new Lesson
        {
            LessonId = lessonId,
            CourseId = courseId
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync((ModuleCompletion?)null);

        _moduleCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<ModuleCompletion>()))
            .Returns(Task.CompletedTask);

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdAsync(lessonId))
            .ReturnsAsync(new List<Module> { module });

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ModuleCompletion>());

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ReturnsAsync((LessonCompletion?)null);

        _lessonCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<LessonCompletion>()))
            .Returns(Task.CompletedTask);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(lessonId))
            .ReturnsAsync(lesson);

        _lessonRepositoryMock
            .Setup(x => x.GetListLessonByCourseId(courseId))
            .ReturnsAsync(new List<Lesson> { lesson });

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<LessonCompletion>());

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(courseId))
            .ReturnsAsync(new Course { CourseId = courseId });

        _courseProgressRepositoryMock
            .Setup(x => x.GetByUserAndCourseAsync(userId, courseId))
            .ReturnsAsync((CourseProgress?)null);

        _courseProgressRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<CourseProgress>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        // Should auto-complete for these types
        _moduleCompletionRepositoryMock.Verify(x => x.AddAsync(It.Is<ModuleCompletion>(mc => mc.IsCompleted)), Times.Once);
    }

    #endregion

    #region UpdateVideoProgressAsync Tests

    [Fact]
    public async Task UpdateVideoProgressAsync_WithNewCompletion_CreatesNew()
    {
        // Arrange
        var userId = 1;
        var lessonId = 1;
        var positionSeconds = 120;
        var videoPercentage = 50.0f;

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ReturnsAsync((LessonCompletion?)null);

        _lessonCompletionRepositoryMock
            .Setup(x => x.AddAsync(It.Is<LessonCompletion>(lc =>
                lc.UserId == userId &&
                lc.LessonId == lessonId &&
                lc.VideoProgressPercentage == videoPercentage)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.UpdateVideoProgressAsync(userId, lessonId, positionSeconds, videoPercentage);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.Contains("Video progress updated successfully", result.Message);

        _lessonCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LessonCompletion>()), Times.Once);
        _lessonCompletionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<LessonCompletion>()), Times.Never);
    }

    [Fact]
    public async Task UpdateVideoProgressAsync_WithExistingCompletion_Updates()
    {
        // Arrange
        var userId = 1;
        var lessonId = 1;
        var positionSeconds = 180;
        var videoPercentage = 75.0f;

        var existingCompletion = new LessonCompletion
        {
            LessonCompletionId = 1,
            UserId = userId,
            LessonId = lessonId,
            VideoProgressPercentage = 25.0f
        };

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ReturnsAsync(existingCompletion);

        _lessonCompletionRepositoryMock
            .Setup(x => x.UpdateAsync(It.Is<LessonCompletion>(lc =>
                lc.VideoProgressPercentage == videoPercentage)))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _moduleProgressService.UpdateVideoProgressAsync(userId, lessonId, positionSeconds, videoPercentage);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);

        _lessonCompletionRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<LessonCompletion>()), Times.Once);
        _lessonCompletionRepositoryMock.Verify(x => x.AddAsync(It.IsAny<LessonCompletion>()), Times.Never);
    }

    [Fact]
    public async Task UpdateVideoProgressAsync_WithException_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var lessonId = 1;
        var positionSeconds = 120;
        var videoPercentage = 50.0f;

        _lessonCompletionRepositoryMock
            .Setup(x => x.GetByUserAndLessonAsync(userId, lessonId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _moduleProgressService.UpdateVideoProgressAsync(userId, lessonId, positionSeconds, videoPercentage);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi khi cập nhật tiến độ video", result.Message);
    }

    #endregion
}

