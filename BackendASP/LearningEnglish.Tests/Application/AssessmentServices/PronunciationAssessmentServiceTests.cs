using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application.AssessmentServices;

public class PronunciationAssessmentServiceTests
{
    private readonly Mock<IFlashCardRepository> _flashCardRepositoryMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly Mock<IAzureSpeechService> _azureSpeechServiceMock;
    private readonly Mock<IPronunciationProgressRepository> _progressRepositoryMock;
    private readonly Mock<ILogger<PronunciationAssessmentService>> _loggerMock;
    private readonly PronunciationAssessmentService _pronunciationAssessmentService;

    public PronunciationAssessmentServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _flashCardRepositoryMock = new Mock<IFlashCardRepository>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();
        _azureSpeechServiceMock = new Mock<IAzureSpeechService>();
        _progressRepositoryMock = new Mock<IPronunciationProgressRepository>();
        _loggerMock = new Mock<ILogger<PronunciationAssessmentService>>();

        _pronunciationAssessmentService = new PronunciationAssessmentService(
            _flashCardRepositoryMock.Object,
            _minioFileStorageMock.Object,
            _azureSpeechServiceMock.Object,
            _progressRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region CreateAssessmentAsync Tests

    [Fact]
    public async Task CreateAssessmentAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new CreatePronunciationAssessmentDto
        {
            FlashCardId = 1,
            AudioTempKey = "temp/audio-key",
            AudioType = "audio/mpeg",
            AudioSize = 1024,
            DurationInSeconds = 2.5F
        };

        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = "hello"
        };

        var azureResult = new AzureSpeechAssessmentResult
        {
            Success = true,
            PronunciationScore = 85.5F,
            AccuracyScore = 90F,
            FluencyScore = 80F,
            CompletenessScore = 90F,
            RecognizedText = "hello",
            ProblemPhonemes = new List<string>(),
            StrongPhonemes = new List<string> { "h", "l", "o" }
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.FlashCardId))
            .ReturnsAsync(flashCard);

        _azureSpeechServiceMock
            .Setup(x => x.AssessPronunciationAsync(It.IsAny<string>(), flashCard.Word, It.IsAny<string>()))
            .ReturnsAsync(azureResult);

        var progress = new PronunciationProgress
        {
            FlashCardId = dto.FlashCardId,
            UserId = userId
        };

        _progressRepositoryMock
            .Setup(x => x.UpsertAsync(
                userId,
                dto.FlashCardId,
                azureResult.AccuracyScore,
                azureResult.FluencyScore,
                azureResult.CompletenessScore,
                azureResult.PronunciationScore,
                azureResult.ProblemPhonemes,
                azureResult.StrongPhonemes,
                It.IsAny<DateTime>()))
            .ReturnsAsync(progress);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _pronunciationAssessmentService.CreateAssessmentAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(flashCard.Word, result.Data.ReferenceText);
        Assert.Equal(azureResult.PronunciationScore, result.Data.PronunciationScore);

        _azureSpeechServiceMock.Verify(x => x.AssessPronunciationAsync(It.IsAny<string>(), flashCard.Word, It.IsAny<string>()), Times.Once);
        _progressRepositoryMock.Verify(x => x.UpsertAsync(
            userId,
            dto.FlashCardId,
            azureResult.AccuracyScore,
            azureResult.FluencyScore,
            azureResult.CompletenessScore,
            azureResult.PronunciationScore,
            azureResult.ProblemPhonemes,
            azureResult.StrongPhonemes,
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task CreateAssessmentAsync_WithNonExistentFlashCard_ReturnsNotFound()
    {
        // Arrange
        var userId = 1;
        var dto = new CreatePronunciationAssessmentDto
        {
            FlashCardId = 999,
            AudioTempKey = "temp/audio-key"
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.FlashCardId))
            .ReturnsAsync((FlashCard?)null);

        // Act
        var result = await _pronunciationAssessmentService.CreateAssessmentAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("FlashCard not found", result.Message);

        _azureSpeechServiceMock.Verify(x => x.AssessPronunciationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CreateAssessmentAsync_WithEmptyWord_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreatePronunciationAssessmentDto
        {
            FlashCardId = 1,
            AudioTempKey = "temp/audio-key"
        };

        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = ""
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.FlashCardId))
            .ReturnsAsync(flashCard);

        // Act
        var result = await _pronunciationAssessmentService.CreateAssessmentAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("does not have a valid word", result.Message);
    }

    #endregion

    #region GetFlashCardsWithPronunciationProgressAsync Tests

    [Fact]
    public async Task GetFlashCardsWithPronunciationProgressAsync_WithFlashCards_ReturnsFlashCardsWithProgress()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var flashCards = new List<FlashCard>
        {
            new FlashCard
            {
                FlashCardId = 1,
                Word = "hello",
                Meaning = "greeting",
                ImageKey = "real/image1.jpg",
                AudioKey = "real/audio1.mp3"
            }
        };

        var progresses = new List<PronunciationProgress>
        {
            new PronunciationProgress
            {
                FlashCardId = 1,
                UserId = userId,
                TotalAttempts = 5,
                BestScore = 90F,
                AvgPronunciationScore = 85F,
                IsMastered = false
            }
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(flashCards);

        _progressRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(userId, moduleId))
            .ReturnsAsync(progresses);

        // Act
        var result = await _pronunciationAssessmentService.GetFlashCardsWithPronunciationProgressAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.NotNull(result.Data[0].Progress);
        Assert.NotNull(result.Data[0].Progress);
        Assert.Equal(5, result.Data[0].Progress!.TotalAttempts);
    }

    [Fact]
    public async Task GetFlashCardsWithPronunciationProgressAsync_WithNoProgress_ReturnsFlashCardsWithoutProgress()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var flashCards = new List<FlashCard>
        {
            new FlashCard
            {
                FlashCardId = 1,
                Word = "hello"
            }
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(flashCards);

        _progressRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(userId, moduleId))
            .ReturnsAsync(new List<PronunciationProgress>());

        // Act
        var result = await _pronunciationAssessmentService.GetFlashCardsWithPronunciationProgressAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
        Assert.Null(result.Data[0].Progress);
    }

    #endregion

    #region GetModulePronunciationSummaryAsync Tests

    [Fact]
    public async Task GetModulePronunciationSummaryAsync_WithFlashCards_ReturnsSummary()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var flashCards = new List<FlashCard>
        {
            new FlashCard
            {
                FlashCardId = 1,
                Word = "hello",
                Module = new Module { ModuleId = moduleId, Name = "Test Module" }
            },
            new FlashCard
            {
                FlashCardId = 2,
                Word = "world",
                Module = new Module { ModuleId = moduleId, Name = "Test Module" }
            }
        };

        var progresses = new List<PronunciationProgress>
        {
            new PronunciationProgress
            {
                FlashCardId = 1,
                UserId = userId,
                TotalAttempts = 5,
                AvgPronunciationScore = 85F,
                IsMastered = false
            }
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(flashCards);

        _progressRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(userId, moduleId))
            .ReturnsAsync(progresses);

        // Act
        var result = await _pronunciationAssessmentService.GetModulePronunciationSummaryAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.TotalFlashCards);
        Assert.Equal(1, result.Data.TotalPracticed);
        Assert.Equal(0, result.Data.MasteredCount);
    }

    [Fact]
    public async Task GetModulePronunciationSummaryAsync_WithNoFlashCards_ReturnsNotFound()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(new List<FlashCard>());

        // Act
        var result = await _pronunciationAssessmentService.GetModulePronunciationSummaryAsync(moduleId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Module không có flashcard nào", result.Message);
    }

    #endregion
}

