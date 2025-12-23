using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application.QuizServices;

public class QuestionServiceTests
{
    private readonly Mock<IQuestionRepository> _questionRepositoryMock;
    private readonly Mock<IQuizGroupRepository> _quizGroupRepositoryMock;
    private readonly Mock<IQuizSectionRepository> _quizSectionRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<QuestionService>> _loggerMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly QuestionService _questionService;

    public QuestionServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _questionRepositoryMock = new Mock<IQuestionRepository>();
        _quizGroupRepositoryMock = new Mock<IQuizGroupRepository>();
        _quizSectionRepositoryMock = new Mock<IQuizSectionRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<QuestionService>>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _questionService = new QuestionService(
            _questionRepositoryMock.Object,
            _quizGroupRepositoryMock.Object,
            _quizSectionRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetQuestionByIdAsync Tests

    [Fact]
    public async Task GetQuestionByIdAsync_WithValidId_ReturnsQuestion()
    {
        // Arrange
        var questionId = 1;
        var question = new Question
        {
            QuestionId = questionId,
            StemText = "Test Question",
            Type = QuestionType.MultipleChoice,
            MediaKey = "questions/real/media-123",
            Options = new List<AnswerOption>
            {
                new AnswerOption { AnswerOptionId = 1, Text = "Option 1", MediaKey = "questions/real/opt1-123" }
            }
        };

        var questionDto = new QuestionReadDto
        {
            QuestionId = questionId,
            StemText = "Test Question",
            Type = QuestionType.MultipleChoice,
            MediaUrl = "questions/real/media-123",
            Options = new List<AnswerOptionReadDto>
            {
                new AnswerOptionReadDto { AnswerOptionId = 1, Text = "Option 1", MediaUrl = "questions/real/opt1-123" }
            }
        };

        _questionRepositoryMock
            .Setup(x => x.GetQuestionByIdAsync(questionId))
            .ReturnsAsync(question);

        _mapperMock
            .Setup(x => x.Map<QuestionReadDto>(question))
            .Returns(questionDto);

        // Act
        var result = await _questionService.GetQuestionByIdAsync(questionId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(questionId, result.Data.QuestionId);
        Assert.NotNull(result.Data.MediaUrl); // Should be built from MediaKey
        Assert.NotNull(result.Data.Options);
        Assert.Single(result.Data.Options);

        _questionRepositoryMock.Verify(x => x.GetQuestionByIdAsync(questionId), Times.Once);
    }

    [Fact]
    public async Task GetQuestionByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var questionId = 999;

        _questionRepositoryMock
            .Setup(x => x.GetQuestionByIdAsync(questionId))
            .ReturnsAsync((Question?)null);

        // Act
        var result = await _questionService.GetQuestionByIdAsync(questionId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy câu hỏi", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetQuestionsByQuizGroupIdAsync Tests

    [Fact]
    public async Task GetQuestionsByQuizGroupIdAsync_WithValidGroupId_ReturnsQuestions()
    {
        // Arrange
        var quizGroupId = 1;
        var questions = new List<Question>
        {
            new Question
            {
                QuestionId = 1,
                StemText = "Question 1",
                QuizGroupId = quizGroupId
            },
            new Question
            {
                QuestionId = 2,
                StemText = "Question 2",
                QuizGroupId = quizGroupId
            }
        };

        var questionDtos = new List<QuestionReadDto>
        {
            new QuestionReadDto
            {
                QuestionId = 1,
                StemText = "Question 1",
                QuizGroupId = quizGroupId
            },
            new QuestionReadDto
            {
                QuestionId = 2,
                StemText = "Question 2",
                QuizGroupId = quizGroupId
            }
        };

        _questionRepositoryMock
            .Setup(x => x.GetQuestionsByQuizGroupIdAsync(quizGroupId))
            .ReturnsAsync(questions);

        _mapperMock
            .Setup(x => x.Map<List<QuestionReadDto>>(questions))
            .Returns(questionDtos);

        // Act
        var result = await _questionService.GetQuestionsByQuizGroupIdAsync(quizGroupId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        _questionRepositoryMock.Verify(x => x.GetQuestionsByQuizGroupIdAsync(quizGroupId), Times.Once);
    }

    #endregion

    #region AddQuestionAsync Tests

    [Fact]
    public async Task AddQuestionAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new QuestionCreateDto
        {
            QuizGroupId = 1,
            QuizSectionId = 1,
            StemText = "New Question",
            Type = QuestionType.MultipleChoice,
            Points = 10,
            Options = new List<AnswerOptionCreateDto>
            {
                new AnswerOptionCreateDto { Text = "Option 1", IsCorrect = true },
                new AnswerOptionCreateDto { Text = "Option 2", IsCorrect = false }
            }
        };

        var quizGroup = new QuizGroup
        {
            QuizGroupId = 1,
            Title = "Test Group"
        };

        var quizSection = new QuizSection
        {
            QuizSectionId = 1,
            Title = "Test Section"
        };

        var question = new Question
        {
            QuestionId = 1,
            StemText = dto.StemText,
            Type = dto.Type,
            QuizGroupId = dto.QuizGroupId,
            QuizSectionId = dto.QuizSectionId,
            Points = dto.Points
        };

        var questionDto = new QuestionReadDto
        {
            QuestionId = 1,
            StemText = dto.StemText,
            Type = dto.Type
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(dto.QuizGroupId))
            .ReturnsAsync(quizGroup);

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<Question>(dto))
            .Returns(question);

        _questionRepositoryMock
            .Setup(x => x.AddQuestionAsync(It.IsAny<Question>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<QuestionReadDto>(It.IsAny<Question>()))
            .Returns(questionDto);

        // Act
        var result = await _questionService.AddQuestionAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.StemText, result.Data.StemText);
        Assert.Contains("Tạo câu hỏi thành công", result.Message);

        _questionRepositoryMock.Verify(x => x.AddQuestionAsync(It.IsAny<Question>()), Times.Once);
    }

    [Fact]
    public async Task AddQuestionAsync_WithNonExistentQuizGroup_ReturnsNotFound()
    {
        // Arrange
        var dto = new QuestionCreateDto
        {
            QuizGroupId = 999,
            QuizSectionId = 1,
            StemText = "New Question",
            Type = QuestionType.MultipleChoice
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(dto.QuizGroupId))
            .ReturnsAsync((QuizGroup?)null);

        // Act
        var result = await _questionService.AddQuestionAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Quiz group không tồn tại", result.Message);

        _questionRepositoryMock.Verify(x => x.AddQuestionAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task AddQuestionAsync_WithNonExistentQuizSection_ReturnsNotFound()
    {
        // Arrange
        var dto = new QuestionCreateDto
        {
            QuizGroupId = 1,
            QuizSectionId = 999,
            StemText = "New Question",
            Type = QuestionType.MultipleChoice
        };

        var quizGroup = new QuizGroup
        {
            QuizGroupId = 1
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(dto.QuizGroupId))
            .ReturnsAsync(quizGroup);

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync((QuizSection?)null);

        // Act
        var result = await _questionService.AddQuestionAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Quiz section không tồn tại", result.Message);

        _questionRepositoryMock.Verify(x => x.AddQuestionAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task AddQuestionAsync_WithMedia_CommitsFiles()
    {
        // Arrange
        var dto = new QuestionCreateDto
        {
            QuizGroupId = 1,
            QuizSectionId = 1,
            StemText = "New Question",
            Type = QuestionType.MultipleChoice,
            MediaTempKey = "temp/question-media-123",
            Options = new List<AnswerOptionCreateDto>
            {
                new AnswerOptionCreateDto
                {
                    Text = "Option 1",
                    MediaTempKey = "temp/option1-media-123"
                }
            }
        };

        var quizGroup = new QuizGroup { QuizGroupId = 1 };
        var quizSection = new QuizSection { QuizSectionId = 1 };
        var question = new Question { QuestionId = 1, StemText = dto.StemText };
        var questionDto = new QuestionReadDto { QuestionId = 1, StemText = dto.StemText };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(dto.QuizGroupId))
            .ReturnsAsync(quizGroup);

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        var mappedQuestion = new Question
        {
            QuestionId = 1,
            StemText = dto.StemText,
            Type = dto.Type,
            QuizGroupId = dto.QuizGroupId,
            QuizSectionId = dto.QuizSectionId,
            Points = dto.Points,
            Options = new List<AnswerOption>
            {
                new AnswerOption { Text = dto.Options[0].Text, IsCorrect = dto.Options[0].IsCorrect }
            }
        };

        _mapperMock
            .Setup(x => x.Map<Question>(dto))
            .Returns(mappedQuestion);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.MediaTempKey!, "questions", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "questions/real/question-media-123"
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.Options[0].MediaTempKey!, "questions", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "questions/real/option1-media-123"
            });

        _questionRepositoryMock
            .Setup(x => x.AddQuestionAsync(It.IsAny<Question>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<QuestionReadDto>(It.IsAny<Question>()))
            .Returns(questionDto);

        // Act
        var result = await _questionService.AddQuestionAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.MediaTempKey, "questions", "real"), Times.Once);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.Options[0].MediaTempKey, "questions", "real"), Times.Once);
    }

    [Fact]
    public async Task AddQuestionAsync_WithOptionMediaCommitFailure_RollsBackQuestionMedia()
    {
        // Arrange
        var dto = new QuestionCreateDto
        {
            QuizGroupId = 1,
            QuizSectionId = 1,
            StemText = "New Question",
            Type = QuestionType.MultipleChoice,
            MediaTempKey = "temp/question-media-123",
            Options = new List<AnswerOptionCreateDto>
            {
                new AnswerOptionCreateDto
                {
                    Text = "Option 1",
                    MediaTempKey = "temp/option1-media-123"
                }
            }
        };

        var quizGroup = new QuizGroup { QuizGroupId = 1 };
        var quizSection = new QuizSection { QuizSectionId = 1 };
        var question = new Question { QuestionId = 1 };

        var committedQuestionKey = "questions/real/question-media-123";

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(dto.QuizGroupId))
            .ReturnsAsync(quizGroup);

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        var mappedQuestion = new Question
        {
            QuestionId = 1,
            StemText = dto.StemText,
            Type = dto.Type,
            QuizGroupId = dto.QuizGroupId,
            QuizSectionId = dto.QuizSectionId,
            Options = new List<AnswerOption>
            {
                new AnswerOption { Text = dto.Options[0].Text }
            }
        };

        _mapperMock
            .Setup(x => x.Map<Question>(dto))
            .Returns(mappedQuestion);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.MediaTempKey!, "questions", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedQuestionKey
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.Options[0].MediaTempKey!, "questions", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit"
            });

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(committedQuestionKey, "questions"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _questionService.AddQuestionAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Không thể lưu media đáp án", result.Message);

        // Should rollback question media
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedQuestionKey, "questions"), Times.Once);
        _questionRepositoryMock.Verify(x => x.AddQuestionAsync(It.IsAny<Question>()), Times.Never);
    }

    [Fact]
    public async Task AddQuestionAsync_WithDatabaseError_RollsBackAllFiles()
    {
        // Arrange
        var dto = new QuestionCreateDto
        {
            QuizGroupId = 1,
            QuizSectionId = 1,
            StemText = "New Question",
            Type = QuestionType.MultipleChoice,
            MediaTempKey = "temp/question-media-123",
            Options = new List<AnswerOptionCreateDto>
            {
                new AnswerOptionCreateDto
                {
                    Text = "Option 1",
                    MediaTempKey = "temp/option1-media-123"
                }
            }
        };

        var quizGroup = new QuizGroup { QuizGroupId = 1 };
        var quizSection = new QuizSection { QuizSectionId = 1 };
        var question = new Question { QuestionId = 1 };

        var committedQuestionKey = "questions/real/question-media-123";
        var committedOptionKey = "questions/real/option1-media-123";

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(dto.QuizGroupId))
            .ReturnsAsync(quizGroup);

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        var mappedQuestion = new Question
        {
            QuestionId = 1,
            StemText = dto.StemText,
            Type = dto.Type,
            QuizGroupId = dto.QuizGroupId,
            QuizSectionId = dto.QuizSectionId,
            Options = new List<AnswerOption>
            {
                new AnswerOption { Text = dto.Options[0].Text }
            }
        };

        _mapperMock
            .Setup(x => x.Map<Question>(dto))
            .Returns(mappedQuestion);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.MediaTempKey!, "questions", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedQuestionKey
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.Options[0].MediaTempKey!, "questions", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedOptionKey
            });

        _questionRepositoryMock
            .Setup(x => x.AddQuestionAsync(It.IsAny<Question>()))
            .ThrowsAsync(new Exception("Database error"));

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), "questions"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _questionService.AddQuestionAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi database", result.Message);

        // Should rollback all files
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedQuestionKey, "questions"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedOptionKey, "questions"), Times.Once);
    }

    #endregion

    #region UpdateQuestionAsync Tests

    [Fact]
    public async Task UpdateQuestionAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var questionId = 1;
        var dto = new QuestionUpdateDto
        {
            StemText = "Updated Question",
            QuizGroupId = 1, // Same as existing
            QuizSectionId = 1 // Same as existing
            // Options không cần set nếu không update options
        };

        var existingQuestion = new Question
        {
            QuestionId = questionId,
            StemText = "Original Question",
            QuizGroupId = 1,
            QuizSectionId = 1,
            Options = new List<AnswerOption>()
        };

        var questionDto = new QuestionReadDto
        {
            QuestionId = questionId,
            StemText = dto.StemText,
            Options = new List<AnswerOptionReadDto>()
        };

        _questionRepositoryMock
            .Setup(x => x.GetQuestionByIdAsync(questionId))
            .ReturnsAsync(existingQuestion);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<QuestionUpdateDto>(), It.IsAny<Question>()))
            .Callback<QuestionUpdateDto, Question>((d, q) =>
            {
                if (!string.IsNullOrEmpty(d.StemText))
                    q.StemText = d.StemText;
            });

        _questionRepositoryMock
            .Setup(x => x.UpdateQuestionAsync(It.IsAny<Question>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<QuestionReadDto>(It.Is<Question>(q => q.QuestionId == questionId && q.StemText == dto.StemText)))
            .Returns(questionDto);

        // Act
        var result = await _questionService.UpdateQuestionAsync(questionId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.StemText, result.Data.StemText);

        _questionRepositoryMock.Verify(x => x.UpdateQuestionAsync(It.IsAny<Question>()), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestionAsync_WithNonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        var questionId = 999;
        var dto = new QuestionUpdateDto
        {
            StemText = "Updated Question"
        };

        _questionRepositoryMock
            .Setup(x => x.GetQuestionByIdAsync(questionId))
            .ReturnsAsync((Question?)null);

        // Act
        var result = await _questionService.UpdateQuestionAsync(questionId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy câu hỏi", result.Message);

        _questionRepositoryMock.Verify(x => x.UpdateQuestionAsync(It.IsAny<Question>()), Times.Never);
    }

    #endregion

    #region DeleteQuestionAsync Tests

    [Fact]
    public async Task DeleteQuestionAsync_WithValidQuestion_ReturnsSuccess()
    {
        // Arrange
        var questionId = 1;

        var question = new Question
        {
            QuestionId = questionId,
            StemText = "Test Question",
            MediaKey = "questions/real/question-media-123",
            Options = new List<AnswerOption>
            {
                new AnswerOption
                {
                    AnswerOptionId = 1,
                    MediaKey = "questions/real/option1-media-123"
                }
            }
        };

        _questionRepositoryMock
            .Setup(x => x.GetQuestionByIdAsync(questionId))
            .ReturnsAsync(question);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), "questions"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        _questionRepositoryMock
            .Setup(x => x.DeleteQuestionAsync(questionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _questionService.DeleteQuestionAsync(questionId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Xóa câu hỏi thành công", result.Message);

        // Should delete question media and all option media
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(question.MediaKey!, "questions"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(question.Options[0].MediaKey!, "questions"), Times.Once);
        _questionRepositoryMock.Verify(x => x.DeleteQuestionAsync(questionId), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestionAsync_WithNonExistentQuestion_ReturnsNotFound()
    {
        // Arrange
        var questionId = 999;

        _questionRepositoryMock
            .Setup(x => x.GetQuestionByIdAsync(questionId))
            .ReturnsAsync((Question?)null);

        // Act
        var result = await _questionService.DeleteQuestionAsync(questionId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy câu hỏi", result.Message);

        _questionRepositoryMock.Verify(x => x.DeleteQuestionAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}

