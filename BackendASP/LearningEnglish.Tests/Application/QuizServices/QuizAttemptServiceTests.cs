using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using AutoMapper;

namespace LearningEnglish.Tests.Application.QuizServices;

public class QuizAttemptServiceTests
{
    private readonly Mock<IQuizRepository> _quizRepositoryMock;
    private readonly Mock<IQuizAttemptRepository> _quizAttemptRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IEnumerable<IScoringStrategy>> _scoringStrategiesMock;
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<IModuleProgressService> _moduleProgressServiceMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly QuizAttemptService _quizAttemptService;

    public QuizAttemptServiceTests()
    {
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _quizAttemptRepositoryMock = new Mock<IQuizAttemptRepository>();
        _mapperMock = new Mock<IMapper>();
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _moduleProgressServiceMock = new Mock<IModuleProgressService>();
        _streakServiceMock = new Mock<IStreakService>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();

        // Setup empty scoring strategies collection
        var scoringStrategies = new List<IScoringStrategy>();
        _scoringStrategiesMock = new Mock<IEnumerable<IScoringStrategy>>();

        _quizAttemptService = new QuizAttemptService(
            _quizRepositoryMock.Object,
            _quizAttemptRepositoryMock.Object,
            _mapperMock.Object,
            _assessmentRepositoryMock.Object,
            _userRepositoryMock.Object,
            scoringStrategies,
            _questionRepositoryMock.Object,
            _moduleProgressServiceMock.Object,
            _streakServiceMock.Object,
            _notificationRepositoryMock.Object
        );
    }

    #region StartQuizAttemptAsync Tests

    [Fact]
    public async Task StartQuizAttemptAsync_WithValidQuiz_ReturnsSuccess()
    {
        // Arrange
        var quizId = 1;
        var userId = 1;

        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            Status = QuizStatus.Open,
            AssessmentId = 1
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            DueAt = DateTime.UtcNow.AddDays(1)
        };

        var questions = new List<Question>
        {
            new Question
            {
                QuestionId = 1,
                StemText = "Question 1",
                Type = QuestionType.MultipleChoice
            }
        };

        var attempt = new QuizAttempt
        {
            AttemptId = 1,
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Status = QuizAttemptStatus.InProgress
        };

        var attemptDto = new QuizAttemptWithQuestionsDto
        {
            AttemptId = 1,
            QuizId = quizId,
            UserId = userId
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(quiz.AssessmentId))
            .ReturnsAsync(assessment);

        _questionRepositoryMock
            .Setup(x => x.GetQuestionsByQuizGroupIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Question>());

        _quizAttemptRepositoryMock
            .Setup(x => x.AddQuizAttemptAsync(It.IsAny<QuizAttempt>()))
            .Returns(Task.CompletedTask);

        // After AddQuizAttemptAsync, service uses the created attempt object directly
        // So we need to setup mapper to use the attempt we created
        _mapperMock
            .Setup(x => x.Map<QuizAttemptWithQuestionsDto>(It.Is<QuizAttempt>(a => a.QuizId == quizId && a.UserId == userId)))
            .Returns(attemptDto);

        // Act
        var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(quizId, result.Data.QuizId);
        Assert.Equal(userId, result.Data.UserId);

        _quizAttemptRepositoryMock.Verify(x => x.AddQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Once);
    }

    [Fact]
    public async Task StartQuizAttemptAsync_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var quizId = 999;
        var userId = 1;

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        // Act
        var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Quiz không tồn tại", result.Message);

        _quizAttemptRepositoryMock.Verify(x => x.AddQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Never);
    }

    [Fact]
    public async Task StartQuizAttemptAsync_WithClosedQuiz_ReturnsForbidden()
    {
        // Arrange
        var quizId = 1;
        var userId = 1;

        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            Status = QuizStatus.Closed
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        // Act
        var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Quiz đã đóng", result.Message);

        _quizAttemptRepositoryMock.Verify(x => x.AddQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Never);
    }

    [Fact]
    public async Task StartQuizAttemptAsync_WithExpiredAssessment_ReturnsForbidden()
    {
        // Arrange
        var quizId = 1;
        var userId = 1;

        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            Status = QuizStatus.Open,
            AssessmentId = 1
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            DueAt = DateTime.UtcNow.AddDays(-1) // Expired
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(quiz.AssessmentId))
            .ReturnsAsync(assessment);

        // Act
        var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Assessment đã quá hạn", result.Message);

        _quizAttemptRepositoryMock.Verify(x => x.AddQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Never);
    }

    #endregion

    #region ResumeQuizAttemptAsync Tests

    [Fact]
    public async Task ResumeQuizAttemptAsync_WithValidAttempt_ReturnsSuccess()
    {
        // Arrange
        var attemptId = 1;

        var attempt = new QuizAttempt
        {
            AttemptId = attemptId,
            QuizId = 1,
            UserId = 1,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var quiz = new Quiz
        {
            QuizId = 1,
            Title = "Test Quiz",
            Status = QuizStatus.Open
        };

        var questions = new List<Question>
        {
            new Question
            {
                QuestionId = 1,
                StemText = "Question 1"
            }
        };

        var attemptDto = new QuizAttemptWithQuestionsDto
        {
            AttemptId = attemptId,
            QuizId = 1,
            UserId = 1
        };

        _quizAttemptRepositoryMock
            .Setup(x => x.GetByIdAsync(attemptId))
            .ReturnsAsync(attempt);

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(attempt.QuizId))
            .ReturnsAsync(quiz);

        _quizRepositoryMock
            .Setup(x => x.GetFullQuizAsync(attempt.QuizId))
            .ReturnsAsync(quiz);

        _mapperMock
            .Setup(x => x.Map<QuizAttemptWithQuestionsDto>(It.IsAny<QuizAttempt>()))
            .Returns(attemptDto);

        // Act
        var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(attemptId, result.Data.AttemptId);
    }

    [Fact]
    public async Task ResumeQuizAttemptAsync_WithNonExistentAttempt_ReturnsNotFound()
    {
        // Arrange
        var attemptId = 999;

        _quizAttemptRepositoryMock
            .Setup(x => x.GetByIdAsync(attemptId))
            .ReturnsAsync((QuizAttempt?)null);

        // Act
        var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy attempt", result.Message);
    }

    [Fact]
    public async Task ResumeQuizAttemptAsync_WithCompletedAttempt_ReturnsError()
    {
        // Arrange
        var attemptId = 1;

        var attempt = new QuizAttempt
        {
            AttemptId = attemptId,
            QuizId = 1,
            UserId = 1,
            Status = QuizAttemptStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        _quizAttemptRepositoryMock
            .Setup(x => x.GetByIdAsync(attemptId))
            .ReturnsAsync(attempt);

        // Act
        var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Attempt đã hoàn thành", result.Message);
    }

    #endregion

    #region SubmitQuizAttemptAsync Tests

    [Fact]
    public async Task SubmitQuizAttemptAsync_WithValidAttempt_ReturnsSuccess()
    {
        // Arrange
        var attemptId = 1;

        var attempt = new QuizAttempt
        {
            AttemptId = attemptId,
            QuizId = 1,
            UserId = 1,
            Status = QuizAttemptStatus.InProgress,
            StartedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var quiz = new Quiz
        {
            QuizId = 1,
            Title = "Test Quiz",
            AssessmentId = 1
        };

        var attemptDto = new QuizAttemptResultDto
        {
            AttemptId = attemptId,
            TotalScore = 80
        };

        _quizAttemptRepositoryMock
            .Setup(x => x.GetByIdAsync(attemptId))
            .ReturnsAsync(attempt);

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(attempt.QuizId))
            .ReturnsAsync(quiz);

        _quizAttemptRepositoryMock
            .Setup(x => x.UpdateQuizAttemptAsync(It.IsAny<QuizAttempt>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<QuizAttemptResultDto>(It.IsAny<QuizAttempt>()))
            .Returns(attemptDto);

        // Act
        var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(attemptId, result.Data.AttemptId);

        _quizAttemptRepositoryMock.Verify(x => x.UpdateQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Once);
    }

    [Fact]
    public async Task SubmitQuizAttemptAsync_WithNonExistentAttempt_ReturnsNotFound()
    {
        // Arrange
        var attemptId = 999;

        _quizAttemptRepositoryMock
            .Setup(x => x.GetByIdAsync(attemptId))
            .ReturnsAsync((QuizAttempt?)null);

        // Act
        var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy attempt", result.Message);

        _quizAttemptRepositoryMock.Verify(x => x.UpdateQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Never);
    }

    [Fact]
    public async Task SubmitQuizAttemptAsync_WithAlreadyCompletedAttempt_ReturnsError()
    {
        // Arrange
        var attemptId = 1;

        var attempt = new QuizAttempt
        {
            AttemptId = attemptId,
            QuizId = 1,
            UserId = 1,
            Status = QuizAttemptStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        _quizAttemptRepositoryMock
            .Setup(x => x.GetByIdAsync(attemptId))
            .ReturnsAsync(attempt);

        // Act
        var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Attempt đã được nộp", result.Message);

        _quizAttemptRepositoryMock.Verify(x => x.UpdateQuizAttemptAsync(It.IsAny<QuizAttempt>()), Times.Never);
    }

    #endregion
}

