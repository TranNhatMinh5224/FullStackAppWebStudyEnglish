using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.ModuleService;

public class ModuleProgressServiceTests
{
    private readonly Mock<IModuleCompletionRepository> _moduleCompletionRepoMock;
    private readonly Mock<ILessonCompletionRepository> _lessonCompletionRepoMock;
    private readonly Mock<ICourseProgressRepository> _courseProgressRepoMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<ILogger<ModuleProgressService>> _loggerMock;
    private readonly ModuleProgressService _service;

    public ModuleProgressServiceTests()
    {
        _moduleCompletionRepoMock = new Mock<IModuleCompletionRepository>();
        _lessonCompletionRepoMock = new Mock<ILessonCompletionRepository>();
        _courseProgressRepoMock = new Mock<ICourseProgressRepository>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _loggerMock = new Mock<ILogger<ModuleProgressService>>();

        _service = new ModuleProgressService(
            _moduleCompletionRepoMock.Object,
            _lessonCompletionRepoMock.Object,
            _courseProgressRepoMock.Object,
            _moduleRepositoryMock.Object,
            _lessonRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CompleteModuleAsync_FirstTime_UpdatesAllProgress()
    {
        // Arrange
        var userId = 1;
        var moduleId = 10;
        var lessonId = 100;
        var courseId = 1000;
        
        var module = new Module { ModuleId = moduleId, LessonId = lessonId };
        var lesson = new Lesson { LessonId = lessonId, CourseId = courseId };
        
        _moduleRepositoryMock.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(module);
        _moduleCompletionRepoMock.Setup(r => r.GetByUserAndModuleAsync(userId, moduleId)).ReturnsAsync((ModuleCompletion)null);
        _lessonRepositoryMock.Setup(r => r.GetLessonById(lessonId)).ReturnsAsync(lesson);
        
        // Mock data for progress calculation
        _moduleRepositoryMock.Setup(r => r.GetByLessonIdAsync(lessonId)).ReturnsAsync(new List<Module> { module });
        _moduleCompletionRepoMock.Setup(r => r.GetByUserAndModuleIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ModuleCompletion> { new ModuleCompletion { ModuleId = moduleId, IsCompleted = true } });
        
        _lessonRepositoryMock.Setup(r => r.GetListLessonByCourseId(courseId)).ReturnsAsync(new List<Lesson> { lesson });
        _lessonCompletionRepoMock.Setup(r => r.GetByUserAndLessonIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<LessonCompletion> { new LessonCompletion { LessonId = lessonId, IsCompleted = true } });

        // Act
        var result = await _service.CompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        _moduleCompletionRepoMock.Verify(r => r.AddAsync(It.Is<ModuleCompletion>(mc => mc.IsCompleted)), Times.Once);
        _lessonCompletionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<LessonCompletion>()) ?? r.AddAsync(It.IsAny<LessonCompletion>()), Times.Once);
        _courseProgressRepoMock.Verify(r => r.UpdateAsync(It.IsAny<CourseProgress>()) ?? r.AddAsync(It.IsAny<CourseProgress>()), Times.Once);
    }

    [Fact]
    public async Task StartAndCompleteModuleAsync_LectureType_AutoCompletes()
    {
        // Arrange
        var userId = 1;
        var moduleId = 10;
        var module = new Module { ModuleId = moduleId, ContentType = ModuleType.Lecture, LessonId = 100 };
        
        _moduleRepositoryMock.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(module);
        _moduleCompletionRepoMock.Setup(r => r.GetByUserAndModuleAsync(userId, moduleId)).ReturnsAsync((ModuleCompletion)null);
        _lessonRepositoryMock.Setup(r => r.GetLessonById(It.IsAny<int>())).ReturnsAsync(new Lesson { CourseId = 1 });

        // Act
        var result = await _service.StartAndCompleteModuleAsync(userId, moduleId);

        // Assert
        Assert.True(result.Success);
        // Verify StartModule logic
        _moduleCompletionRepoMock.Verify(r => r.AddAsync(It.IsAny<ModuleCompletion>()), Times.AtLeastOnce);
        // Verify auto-complete logic was called (CompleteModuleAsync calls repo.Add/Update)
        _moduleCompletionRepoMock.Verify(r => r.AddAsync(It.Is<ModuleCompletion>(mc => mc.IsCompleted)), Times.AtLeastOnce);
    }

    [Fact]
    public async Task UpdateVideoProgressAsync_Success_UpdatesRepo()
    {
        // Arrange
        var userId = 1;
        var lessonId = 100;
        _lessonCompletionRepoMock.Setup(r => r.GetByUserAndLessonAsync(userId, lessonId)).ReturnsAsync((LessonCompletion)null);

        // Act
        var result = await _service.UpdateVideoProgressAsync(userId, lessonId, 120, 0.5f);

        // Assert
        Assert.True(result.Success);
        _lessonCompletionRepoMock.Verify(r => r.AddAsync(It.Is<LessonCompletion>(lc => lc.VideoProgressPercentage == 0.5f)), Times.Once);
    }
}
