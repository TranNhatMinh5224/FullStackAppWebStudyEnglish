using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using AutoMapper;

namespace LearningEnglish.Tests.Application;

public class QuizServiceTests
{
    private readonly Mock<IQuizRepository> _quizRepositoryMock;
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly QuizService _quizService;

    public QuizServiceTests()
    {
        _quizRepositoryMock = new Mock<IQuizRepository>();
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _mapperMock = new Mock<IMapper>();

        _quizService = new QuizService(
            _quizRepositoryMock.Object,
            _assessmentRepositoryMock.Object,
            _mapperMock.Object
        );
    }

    #region GetQuizByIdAsync Tests

    [Fact]
    public async Task GetQuizByIdAsync_WithValidId_ReturnsQuiz()
    {
        // Arrange
        var quizId = 1;
        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            AssessmentId = 1,
            Type = QuizType.Practice,
            Status = QuizStatus.Open
        };

        var quizDto = new QuizDto
        {
            QuizId = quizId,
            Title = "Test Quiz",
            AssessmentId = 1,
            Type = QuizType.Practice,
            Status = QuizStatus.Open
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _mapperMock
            .Setup(x => x.Map<QuizDto>(quiz))
            .Returns(quizDto);

        // Act
        var result = await _quizService.GetQuizByIdAsync(quizId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(quizId, result.Data.QuizId);
        Assert.Equal("Test Quiz", result.Data.Title);

        _quizRepositoryMock.Verify(x => x.GetQuizByIdAsync(quizId), Times.Once);
    }

    [Fact]
    public async Task GetQuizByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var quizId = 999;

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        // Act
        var result = await _quizService.GetQuizByIdAsync(quizId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Quiz not found", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetQuizzesByAssessmentIdAsync Tests

    [Fact]
    public async Task GetQuizzesByAssessmentIdAsync_WithValidAssessmentId_ReturnsQuizzes()
    {
        // Arrange
        var assessmentId = 1;
        var quizzes = new List<Quiz>
        {
            new Quiz
            {
                QuizId = 1,
                Title = "Quiz 1",
                AssessmentId = assessmentId
            },
            new Quiz
            {
                QuizId = 2,
                Title = "Quiz 2",
                AssessmentId = assessmentId
            }
        };

        var quizDtos = new List<QuizDto>
        {
            new QuizDto
            {
                QuizId = 1,
                Title = "Quiz 1",
                AssessmentId = assessmentId
            },
            new QuizDto
            {
                QuizId = 2,
                Title = "Quiz 2",
                AssessmentId = assessmentId
            }
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizzesByAssessmentIdAsync(assessmentId))
            .ReturnsAsync(quizzes);

        _mapperMock
            .Setup(x => x.Map<List<QuizDto>>(quizzes))
            .Returns(quizDtos);

        // Act
        var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        _quizRepositoryMock.Verify(x => x.GetQuizzesByAssessmentIdAsync(assessmentId), Times.Once);
    }

    [Fact]
    public async Task GetQuizzesByAssessmentIdAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var assessmentId = 1;

        _quizRepositoryMock
            .Setup(x => x.GetQuizzesByAssessmentIdAsync(assessmentId))
            .ReturnsAsync(new List<Quiz>());

        _mapperMock
            .Setup(x => x.Map<List<QuizDto>>(It.IsAny<List<Quiz>>()))
            .Returns(new List<QuizDto>());

        // Act
        var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region CreateQuizAsync Tests

    [Fact]
    public async Task CreateQuizAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new QuizCreateDto
        {
            AssessmentId = 1,
            Title = "New Quiz",
            Type = QuizType.Practice,
            Status = QuizStatus.Open
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            Title = "Test Assessment"
        };

        var quiz = new Quiz
        {
            QuizId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId,
            Type = dto.Type,
            Status = dto.Status
        };

        var quizDto = new QuizDto
        {
            QuizId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId,
            Type = dto.Type,
            Status = dto.Status
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(dto.AssessmentId))
            .ReturnsAsync(assessment);

        _mapperMock
            .Setup(x => x.Map<Quiz>(dto))
            .Returns(quiz);

        _quizRepositoryMock
            .Setup(x => x.AddQuizAsync(It.IsAny<Quiz>()))
            .Returns(Task.CompletedTask);

        _quizRepositoryMock
            .Setup(x => x.GetFullQuizAsync(It.IsAny<int>()))
            .ReturnsAsync((Quiz?)null);

        _mapperMock
            .Setup(x => x.Map<QuizDto>(It.IsAny<Quiz>()))
            .Returns(quizDto);

        // Act
        var result = await _quizService.CreateQuizAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);

        _quizRepositoryMock.Verify(x => x.AddQuizAsync(It.IsAny<Quiz>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuizAsync_WithNonExistentAssessment_ReturnsNotFound()
    {
        // Arrange
        var dto = new QuizCreateDto
        {
            AssessmentId = 999,
            Title = "New Quiz"
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(dto.AssessmentId))
            .ReturnsAsync((Assessment?)null);

        // Act
        var result = await _quizService.CreateQuizAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Assessment not found", result.Message);

        _quizRepositoryMock.Verify(x => x.AddQuizAsync(It.IsAny<Quiz>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuizAsync_AsTeacherWithOwnAssessment_ReturnsSuccess()
    {
        // Arrange
        var teacherId = 1;
        var dto = new QuizCreateDto
        {
            AssessmentId = 1,
            Title = "New Quiz",
            Type = QuizType.Practice
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            Title = "Test Assessment"
        };

        var quiz = new Quiz
        {
            QuizId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId
        };

        var quizDto = new QuizDto
        {
            QuizId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(dto.AssessmentId))
            .ReturnsAsync(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, dto.AssessmentId))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map<Quiz>(dto))
            .Returns(quiz);

        _quizRepositoryMock
            .Setup(x => x.AddQuizAsync(It.IsAny<Quiz>()))
            .Returns(Task.CompletedTask);

        _quizRepositoryMock
            .Setup(x => x.GetFullQuizAsync(It.IsAny<int>()))
            .ReturnsAsync((Quiz?)null);

        _mapperMock
            .Setup(x => x.Map<QuizDto>(It.IsAny<Quiz>()))
            .Returns(quizDto);

        // Act
        var result = await _quizService.CreateQuizAsync(dto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateQuizAsync_AsTeacherWithWrongAssessment_ReturnsForbidden()
    {
        // Arrange
        var teacherId = 1;
        var dto = new QuizCreateDto
        {
            AssessmentId = 1,
            Title = "New Quiz"
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            Title = "Test Assessment"
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(dto.AssessmentId))
            .ReturnsAsync(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, dto.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _quizService.CreateQuizAsync(dto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền tạo Quiz", result.Message);

        _quizRepositoryMock.Verify(x => x.AddQuizAsync(It.IsAny<Quiz>()), Times.Never);
    }

    #endregion

    #region UpdateQuizAsync Tests

    [Fact]
    public async Task UpdateQuizAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var quizId = 1;
        var dto = new QuizUpdateDto
        {
            Title = "Updated Quiz"
        };

        var existingQuiz = new Quiz
        {
            QuizId = quizId,
            Title = "Original Quiz",
            AssessmentId = 1
        };

        var updatedQuiz = new Quiz
        {
            QuizId = quizId,
            Title = dto.Title,
            AssessmentId = 1
        };

        var quizDto = new QuizDto
        {
            QuizId = quizId,
            Title = dto.Title,
            AssessmentId = 1
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(existingQuiz);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<QuizUpdateDto>(), It.IsAny<Quiz>()))
            .Returns(updatedQuiz);

        _quizRepositoryMock
            .Setup(x => x.UpdateQuizAsync(It.IsAny<Quiz>()))
            .Returns(Task.CompletedTask);

        _quizRepositoryMock
            .Setup(x => x.GetFullQuizAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        _mapperMock
            .Setup(x => x.Map<QuizDto>(It.IsAny<Quiz>()))
            .Returns(quizDto);

        // Act
        var result = await _quizService.UpdateQuizAsync(quizId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);

        _quizRepositoryMock.Verify(x => x.UpdateQuizAsync(It.IsAny<Quiz>()), Times.Once);
    }

    [Fact]
    public async Task UpdateQuizAsync_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var quizId = 999;
        var dto = new QuizUpdateDto
        {
            Title = "Updated Quiz"
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        // Act
        var result = await _quizService.UpdateQuizAsync(quizId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Quiz not found", result.Message);

        _quizRepositoryMock.Verify(x => x.UpdateQuizAsync(It.IsAny<Quiz>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuizAsync_AsTeacherWithOwnAssessment_ReturnsSuccess()
    {
        // Arrange
        var quizId = 1;
        var teacherId = 1;
        var dto = new QuizUpdateDto
        {
            Title = "Updated Quiz"
        };

        var existingQuiz = new Quiz
        {
            QuizId = quizId,
            Title = "Original Quiz",
            AssessmentId = 1
        };

        var updatedQuiz = new Quiz
        {
            QuizId = quizId,
            Title = dto.Title,
            AssessmentId = 1
        };

        var quizDto = new QuizDto
        {
            QuizId = quizId,
            Title = dto.Title
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(existingQuiz);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, existingQuiz.AssessmentId))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<QuizUpdateDto>(), It.IsAny<Quiz>()))
            .Returns(updatedQuiz);

        _quizRepositoryMock
            .Setup(x => x.UpdateQuizAsync(It.IsAny<Quiz>()))
            .Returns(Task.CompletedTask);

        _quizRepositoryMock
            .Setup(x => x.GetFullQuizAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        _mapperMock
            .Setup(x => x.Map<QuizDto>(It.IsAny<Quiz>()))
            .Returns(quizDto);

        // Act
        var result = await _quizService.UpdateQuizAsync(quizId, dto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateQuizAsync_AsTeacherWithWrongAssessment_ReturnsForbidden()
    {
        // Arrange
        var quizId = 1;
        var teacherId = 1;
        var dto = new QuizUpdateDto
        {
            Title = "Updated Quiz"
        };

        var existingQuiz = new Quiz
        {
            QuizId = quizId,
            Title = "Original Quiz",
            AssessmentId = 1
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(existingQuiz);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, existingQuiz.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _quizService.UpdateQuizAsync(quizId, dto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền cập nhật Quiz", result.Message);

        _quizRepositoryMock.Verify(x => x.UpdateQuizAsync(It.IsAny<Quiz>()), Times.Never);
    }

    #endregion

    #region DeleteQuizAsync Tests

    [Fact]
    public async Task DeleteQuizAsync_WithValidQuiz_ReturnsSuccess()
    {
        // Arrange
        var quizId = 1;

        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            AssessmentId = 1
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _quizRepositoryMock
            .Setup(x => x.DeleteQuizAsync(quizId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _quizService.DeleteQuizAsync(quizId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);

        _quizRepositoryMock.Verify(x => x.DeleteQuizAsync(quizId), Times.Once);
    }

    [Fact]
    public async Task DeleteQuizAsync_WithNonExistentQuiz_ReturnsNotFound()
    {
        // Arrange
        var quizId = 999;

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync((Quiz?)null);

        // Act
        var result = await _quizService.DeleteQuizAsync(quizId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Quiz not found", result.Message);

        _quizRepositoryMock.Verify(x => x.DeleteQuizAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteQuizAsync_AsTeacherWithOwnAssessment_ReturnsSuccess()
    {
        // Arrange
        var quizId = 1;
        var teacherId = 1;

        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            AssessmentId = 1
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, quiz.AssessmentId))
            .ReturnsAsync(true);

        _quizRepositoryMock
            .Setup(x => x.DeleteQuizAsync(quizId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _quizService.DeleteQuizAsync(quizId, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteQuizAsync_AsTeacherWithWrongAssessment_ReturnsForbidden()
    {
        // Arrange
        var quizId = 1;
        var teacherId = 1;

        var quiz = new Quiz
        {
            QuizId = quizId,
            Title = "Test Quiz",
            AssessmentId = 1
        };

        _quizRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(quizId))
            .ReturnsAsync(quiz);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, quiz.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _quizService.DeleteQuizAsync(quizId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền xóa Quiz", result.Message);

        _quizRepositoryMock.Verify(x => x.DeleteQuizAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}

