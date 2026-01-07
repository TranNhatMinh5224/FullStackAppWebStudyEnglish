using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.EssayService;

public class UserEssaySubmissionServiceTests
{
    private readonly Mock<IEssaySubmissionRepository> _essaySubmissionRepositoryMock;
    private readonly Mock<IEssayRepository> _essayRepositoryMock;
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IModuleProgressService> _moduleProgressServiceMock;
    private readonly Mock<IEssayAttachmentService> _attachmentServiceMock;
    private readonly Mock<IGeminiService> _geminiServiceMock;
    private readonly Mock<IAiResponseParser> _responseParserMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UserEssaySubmissionService>> _loggerMock;
    private readonly UserEssaySubmissionService _service;

    public UserEssaySubmissionServiceTests()
    {
        _essaySubmissionRepositoryMock = new Mock<IEssaySubmissionRepository>();
        _essayRepositoryMock = new Mock<IEssayRepository>();
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _moduleProgressServiceMock = new Mock<IModuleProgressService>();
        _attachmentServiceMock = new Mock<IEssayAttachmentService>();
        _geminiServiceMock = new Mock<IGeminiService>();
        _responseParserMock = new Mock<IAiResponseParser>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserEssaySubmissionService>>();

        _service = new UserEssaySubmissionService(
            _essaySubmissionRepositoryMock.Object,
            _essayRepositoryMock.Object,
            _assessmentRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _moduleProgressServiceMock.Object,
            _attachmentServiceMock.Object,
            _geminiServiceMock.Object,
            _responseParserMock.Object,
            _courseRepositoryMock.Object,
            _moduleRepositoryMock.Object,
            _lessonRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateSubmissionAsync_UserNotEnrolled_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateEssaySubmissionDto { EssayId = 1 };
        var essay = new Essay { EssayId = 1, AssessmentId = 1 };
        var assessment = new Assessment { AssessmentId = 1, ModuleId = 1 };
        var module = new Module { ModuleId = 1, Lesson = new Lesson { CourseId = 1 } };

        _essayRepositoryMock.Setup(r => r.GetEssayByIdAsync(dto.EssayId)).ReturnsAsync(essay);
        _assessmentRepositoryMock.Setup(r => r.GetAssessmentById(essay.AssessmentId)).ReturnsAsync(assessment);
        _moduleRepositoryMock.Setup(r => r.GetModuleWithCourseAsync(assessment.ModuleId)).ReturnsAsync(module);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(1, userId)).ReturnsAsync(false);

        // Act
        var result = await _service.CreateSubmissionAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("đăng ký khóa học", result.Message);
    }

    [Fact]
    public async Task CreateSubmissionAsync_Success_ReturnsCreated()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateEssaySubmissionDto { EssayId = 1, TextContent = "My Essay" };
        var essay = new Essay { EssayId = 1, AssessmentId = 1, Title = "Test Essay" };
        var assessment = new Assessment { AssessmentId = 1, ModuleId = 1 };
        var module = new Module { ModuleId = 1, Lesson = new Lesson { CourseId = 1 } };

        _essayRepositoryMock.Setup(r => r.GetEssayByIdAsync(dto.EssayId)).ReturnsAsync(essay);
        _assessmentRepositoryMock.Setup(r => r.GetAssessmentById(essay.AssessmentId)).ReturnsAsync(assessment);
        _moduleRepositoryMock.Setup(r => r.GetModuleWithCourseAsync(assessment.ModuleId)).ReturnsAsync(module);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(1, userId)).ReturnsAsync(true);
        _essaySubmissionRepositoryMock.Setup(r => r.GetUserSubmissionForEssayAsync(userId, dto.EssayId)).ReturnsAsync((EssaySubmission)null);
        
        var createdSubmission = new EssaySubmission { SubmissionId = 100 };
        _essaySubmissionRepositoryMock.Setup(r => r.CreateSubmissionAsync(It.IsAny<EssaySubmission>())).ReturnsAsync(createdSubmission);
        _mapperMock.Setup(m => m.Map<EssaySubmissionDto>(createdSubmission)).Returns(new EssaySubmissionDto { SubmissionId = 100 });

        // Act
        var result = await _service.CreateSubmissionAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _notificationRepositoryMock.Verify(n => n.AddAsync(It.IsAny<Notification>()), Times.Once);
        _moduleProgressServiceMock.Verify(m => m.CompleteModuleAsync(userId, 1), Times.Once);
    }

    [Fact]
    public async Task RequestAiGradingAsync_TeacherCourse_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var submissionId = 10;
        var submission = new EssaySubmission { SubmissionId = submissionId, UserId = userId, EssayId = 1, TextContent = "Text" };
        var essay = new Essay { EssayId = 1, AssessmentId = 1 };
        var assessment = new Assessment { AssessmentId = 1, Module = new Module { Lesson = new Lesson { Course = new Course { Type = CourseType.Teacher } } } };
        essay.Assessment = assessment;

        _essaySubmissionRepositoryMock.Setup(r => r.GetSubmissionByIdAsync(submissionId)).ReturnsAsync(submission);
        _essayRepositoryMock.Setup(r => r.GetEssayByIdAsync(1)).ReturnsAsync(essay);
        _assessmentRepositoryMock.Setup(r => r.GetAssessmentById(1)).ReturnsAsync(assessment);

        // Act
        var result = await _service.RequestAiGradingAsync(submissionId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("khóa học hệ thống", result.Message);
    }

    [Fact]
    public async Task RequestAiGradingAsync_Success_ReturnsGradingResult()
    {
        // Arrange
        var userId = 1;
        var submissionId = 10;
        var submission = new EssaySubmission { SubmissionId = submissionId, UserId = userId, EssayId = 1, TextContent = "Good text", Status = SubmissionStatus.Submitted };
        var essay = new Essay { EssayId = 1, AssessmentId = 1, Title = "Title", TotalPoints = 100 };
        var assessment = new Assessment { AssessmentId = 1, Module = new Module { Lesson = new Lesson { Course = new Course { Type = CourseType.System } } } };
        essay.Assessment = assessment;

        _essaySubmissionRepositoryMock.Setup(r => r.GetSubmissionByIdAsync(submissionId)).ReturnsAsync(submission);
        _essayRepositoryMock.Setup(r => r.GetEssayByIdAsync(1)).ReturnsAsync(essay);
        _assessmentRepositoryMock.Setup(r => r.GetAssessmentById(1)).ReturnsAsync(assessment);
        
        _geminiServiceMock.Setup(s => s.GenerateContentAsync(It.IsAny<string>()))
            .ReturnsAsync(new GeminiResponse { Success = true, Content = "AI Response" });
        
        _responseParserMock.Setup(p => p.ParseGradingResponse("AI Response"))
            .Returns(new AiGradingResult { Score = 85, Feedback = "Good" });

        // Act
        var result = await _service.RequestAiGradingAsync(submissionId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(85, result.Data.Score);
        Assert.Equal("Good", result.Data.Feedback);
        _essaySubmissionRepositoryMock.Verify(r => r.UpdateSubmissionAsync(It.Is<EssaySubmission>(s => s.Status == SubmissionStatus.Graded)), Times.Once);
    }
}
