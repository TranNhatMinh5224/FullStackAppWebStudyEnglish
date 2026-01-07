using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.QuizzServices;

public class QuizAttemptServiceTests
{
    private readonly Mock<IQuizRepository> _quizRepositoryMock;
    private readonly Mock<IQuizAttemptRepository> _quizAttemptRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<IModuleProgressService> _moduleProgressServiceMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ILogger<QuizAttemptService>> _loggerMock;
    private readonly List<Mock<IScoringStrategy>> _strategyMocks;
    private readonly QuizAttemptService _service;

    public QuizAttemptServiceTests()
    {
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _quizAttemptRepositoryMock = new Mock<IQuizAttemptRepository>();
        _mapperMock = new Mock<IMapper>();
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _moduleProgressServiceMock = new Mock<IModuleProgressService>();
        _streakServiceMock = new Mock<IStreakService>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _loggerMock = new Mock<ILogger<QuizAttemptService>>();

        // Setup Strategy Pattern mocks
        var mcStrategy = new Mock<IScoringStrategy>();
        mcStrategy.Setup(s => s.Type).Returns(QuestionType.MultipleChoice);
        
        _strategyMocks = new List<Mock<IScoringStrategy>> { mcStrategy };
        var strategies = _strategyMocks.Select(m => m.Object);

        _service = new QuizAttemptService(
            _quizRepositoryMock.Object,
            _quizAttemptRepositoryMock.Object,
            _mapperMock.Object,
            _assessmentRepositoryMock.Object,
            _moduleRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _userRepositoryMock.Object,
            strategies,
            _questionRepositoryMock.Object,
            _moduleProgressServiceMock.Object,
            _streakServiceMock.Object,
            _notificationRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task StartQuizAttemptAsync_UserNotEnrolled_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var quizId = 1;
        var quiz = new Quiz { QuizId = quizId, AssessmentId = 1, Status = QuizStatus.Published };
        var assessment = new Assessment { AssessmentId = 1, ModuleId = 1 };
        var module = new Module { ModuleId = 1, Lesson = new Lesson { CourseId = 1 } };

        _quizRepositoryMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(quiz);
        _assessmentRepositoryMock.Setup(r => r.GetAssessmentById(1)).ReturnsAsync(assessment);
        _moduleRepositoryMock.Setup(r => r.GetModuleWithCourseAsync(1)).ReturnsAsync(module);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(1, userId)).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User());

        // Act
        var result = await _service.StartQuizAttemptAsync(quizId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("đăng ký khóa học", result.Message);
    }

    [Fact]
    public async Task UpdateAnswerAndScoreAsync_Success_UsesCorrectStrategy()
    {
        // Arrange
        var userId = 1;
        var attemptId = 10;
        var questionId = 5;
        var attempt = new QuizAttempt { AttemptId = attemptId, UserId = userId, Status = QuizAttemptStatus.InProgress };
        var question = new Question { QuestionId = questionId, Type = QuestionType.MultipleChoice };
        var request = new UpdateAnswerRequestDto { QuestionId = questionId, UserAnswer = "A" };

        _quizAttemptRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(attemptId, userId)).ReturnsAsync(attempt);
        _questionRepositoryMock.Setup(r => r.GetQuestionByIdAsync(questionId)).ReturnsAsync(question);
        
        var strategyMock = _strategyMocks.First();
        strategyMock.Setup(s => s.CalculateScore(question, "A")).Returns(10m);

        // Act
        var result = await _service.UpdateAnswerAndScoreAsync(attemptId, request, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(10, result.Data);
        _quizAttemptRepositoryMock.Verify(r => r.UpdateQuizAttemptAsync(It.Is<QuizAttempt>(a => a.TotalScore == 10)), Times.Once);
    }

    [Fact]
    public async Task SubmitQuizAttemptAsync_Success_MarksModuleComplete()
    {
        // Arrange
        var userId = 1;
        var attemptId = 10;
        var quizId = 1;
        var attempt = new QuizAttempt { AttemptId = attemptId, UserId = userId, QuizId = quizId, Status = QuizAttemptStatus.InProgress, StartedAt = DateTime.UtcNow.AddMinutes(-10) };
        var quiz = new Quiz { QuizId = quizId, AssessmentId = 1, TotalQuestions = 5, ShowScoreImmediately = true };
        var assessment = new Assessment { AssessmentId = 1, ModuleId = 1 };

        _quizAttemptRepositoryMock.Setup(r => r.GetByIdAndUserIdAsync(attemptId, userId)).ReturnsAsync(attempt);
        _quizRepositoryMock.Setup(r => r.GetQuizByIdAsync(quizId)).ReturnsAsync(quiz);
        _assessmentRepositoryMock.Setup(r => r.GetAssessmentById(1)).ReturnsAsync(assessment);

        // Act
        var result = await _service.SubmitQuizAttemptAsync(attemptId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(QuizAttemptStatus.Submitted, attempt.Status);
        _moduleProgressServiceMock.Verify(m => m.CompleteModuleAsync(userId, 1), Times.Once);
        _notificationRepositoryMock.Verify(n => n.AddAsync(It.IsAny<Notification>()), Times.Once);
    }
}
