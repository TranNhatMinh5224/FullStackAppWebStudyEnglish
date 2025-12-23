using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Tests.Application.FlashCardServices;

public class FlashCardReviewServiceTests
{
    private readonly Mock<IFlashCardReviewRepository> _flashCardReviewRepositoryMock;
    private readonly Mock<IFlashCardRepository> _flashCardRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FlashCardReviewService>> _loggerMock;
    private readonly Mock<IStreakService> _streakServiceMock;
    private readonly FlashCardReviewService _flashCardReviewService;

    public FlashCardReviewServiceTests()
    {
        _flashCardReviewRepositoryMock = new Mock<IFlashCardReviewRepository>();
        _flashCardRepositoryMock = new Mock<IFlashCardRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FlashCardReviewService>>();
        _streakServiceMock = new Mock<IStreakService>();

        _flashCardReviewService = new FlashCardReviewService(
            _flashCardReviewRepositoryMock.Object,
            _flashCardRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _streakServiceMock.Object
        );
    }

    #region ReviewFlashCardAsync Tests

    [Fact]
    public async Task ReviewFlashCardAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new ReviewFlashCardDto
        {
            FlashCardId = 1,
            Quality = 5
        };

        var userId = 1;
        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = "Test"
        };

        var review = new FlashCardReview
        {
            FlashCardReviewId = 1,
            FlashCardId = dto.FlashCardId,
            UserId = userId,
            Quality = dto.Quality,
            EasinessFactor = 2.5F,
            IntervalDays = 1,
            RepetitionCount = 0,
            NextReviewDate = DateTime.UtcNow.AddDays(1)
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.FlashCardId))
            .ReturnsAsync(flashCard);

        _flashCardReviewRepositoryMock
            .Setup(x => x.GetReviewAsync(userId, dto.FlashCardId))
            .ReturnsAsync((FlashCardReview?)null);

        _flashCardReviewRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<FlashCardReview>()))
            .ReturnsAsync((FlashCardReview r) => r);

        // Act
        var result = await _flashCardReviewService.ReviewFlashCardAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Quality, result.Data.Quality);

        _flashCardReviewRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FlashCardReview>()), Times.Once);
    }

    [Fact]
    public async Task ReviewFlashCardAsync_WithInvalidQuality_ReturnsBadRequest()
    {
        // Arrange
        var dto = new ReviewFlashCardDto
        {
            FlashCardId = 1,
            Quality = 6 // Invalid, should be 0-5
        };

        var userId = 1;

        // Act
        var result = await _flashCardReviewService.ReviewFlashCardAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Quality phải từ 0-5", result.Message);
    }

    [Fact]
    public async Task ReviewFlashCardAsync_WithNonExistentFlashCard_ReturnsNotFound()
    {
        // Arrange
        var dto = new ReviewFlashCardDto
        {
            FlashCardId = 999,
            Quality = 5
        };

        var userId = 1;

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.FlashCardId))
            .ReturnsAsync((FlashCard?)null);

        // Act
        var result = await _flashCardReviewService.ReviewFlashCardAsync(userId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy flashcard", result.Message);

        _flashCardReviewRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FlashCardReview>()), Times.Never);
    }

    [Fact]
    public async Task ReviewFlashCardAsync_WithExistingReview_UpdatesReview()
    {
        // Arrange
        var dto = new ReviewFlashCardDto
        {
            FlashCardId = 1,
            Quality = 4
        };

        var userId = 1;
        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = "Test"
        };

        var existingReview = new FlashCardReview
        {
            FlashCardReviewId = 1,
            FlashCardId = dto.FlashCardId,
            UserId = userId,
            Quality = 3,
            EasinessFactor = 2.5F,
            IntervalDays = 1,
            RepetitionCount = 1
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.FlashCardId))
            .ReturnsAsync(flashCard);

        _flashCardReviewRepositoryMock
            .Setup(x => x.GetReviewAsync(userId, dto.FlashCardId))
            .ReturnsAsync(existingReview);

        _flashCardReviewRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<FlashCardReview>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _flashCardReviewService.ReviewFlashCardAsync(userId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _flashCardReviewRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FlashCardReview>()), Times.Once);
        _flashCardReviewRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FlashCardReview>()), Times.Never);
    }

    #endregion
}
