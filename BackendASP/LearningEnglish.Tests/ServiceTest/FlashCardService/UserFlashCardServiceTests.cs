using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.FlashCardService;

public class UserFlashCardServiceTests
{
    private readonly Mock<IFlashCardRepository> _flashCardRepositoryMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<UserFlashCardService>> _loggerMock;
    private readonly Mock<IFlashCardMediaService> _flashCardMediaServiceMock;
    private readonly UserFlashCardService _service;

    public UserFlashCardServiceTests()
    {
        _flashCardRepositoryMock = new Mock<IFlashCardRepository>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<UserFlashCardService>>();
        _flashCardMediaServiceMock = new Mock<IFlashCardMediaService>();

        _service = new UserFlashCardService(
            _flashCardRepositoryMock.Object,
            _moduleRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _flashCardMediaServiceMock.Object
        );
    }

    [Fact]
    public async Task GetFlashCardByIdAsync_FlashCardNotFound_ReturnsNotFound()
    {
        // Arrange
        _flashCardRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync((FlashCard)null);

        // Act
        var result = await _service.GetFlashCardByIdAsync(1, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Không tìm thấy FlashCard", result.Message);
    }

    [Fact]
    public async Task GetFlashCardByIdAsync_UserNotEnrolled_ReturnsForbidden()
    {
        // Arrange
        var flashCard = new FlashCard { FlashCardId = 1, Module = new Module { Lesson = new Lesson { CourseId = 100 } } };
        _flashCardRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(flashCard);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(100, 1)).ReturnsAsync(false);

        // Act
        var result = await _service.GetFlashCardByIdAsync(1, 1);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("cần đăng ký khóa học", result.Message);
    }

    [Fact]
    public async Task GetFlashCardByIdAsync_Success_ReturnsDtoWithUrls()
    {
        // Arrange
        var flashCard = new FlashCard { FlashCardId = 1, ImageKey = "img", AudioKey = "aud", Module = new Module { Lesson = new Lesson { CourseId = 100 } } };
        _flashCardRepositoryMock.Setup(r => r.GetByIdWithDetailsAsync(1)).ReturnsAsync(flashCard);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(100, 1)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<FlashCardDto>(flashCard)).Returns(new FlashCardDto());
        _flashCardMediaServiceMock.Setup(s => s.BuildImageUrl("img")).Returns("http://img");
        _flashCardMediaServiceMock.Setup(s => s.BuildAudioUrl("aud")).Returns("http://aud");

        // Act
        var result = await _service.GetFlashCardByIdAsync(1, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("http://img", result.Data.ImageUrl);
        Assert.Equal("http://aud", result.Data.AudioUrl);
    }

    [Fact]
    public async Task GetFlashCardsByModuleIdAsync_Success_ReturnsList()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var module = new Module { ModuleId = moduleId, Lesson = new Lesson { CourseId = 100 } };
        var flashCards = new List<FlashCard> { new FlashCard { FlashCardId = 1, ImageKey = "img" } };
        var flashCardDtos = new List<ListFlashCardDto> { new ListFlashCardDto { FlashCardId = 1 } };

        _moduleRepositoryMock.Setup(r => r.GetModuleWithCourseAsync(moduleId)).ReturnsAsync(module);
        _courseRepositoryMock.Setup(r => r.IsUserEnrolled(100, userId)).ReturnsAsync(true);
        _flashCardRepositoryMock.Setup(r => r.GetByModuleIdWithDetailsAsync(moduleId)).ReturnsAsync(flashCards);
        _mapperMock.Setup(m => m.Map<List<ListFlashCardDto>>(flashCards)).Returns(flashCardDtos);
        _flashCardMediaServiceMock.Setup(s => s.BuildImageUrl("img")).Returns("http://img");

        // Act
        var result = await _service.GetFlashCardsByModuleIdAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.Data);
        Assert.Equal("http://img", result.Data[0].ImageUrl);
    }
}
