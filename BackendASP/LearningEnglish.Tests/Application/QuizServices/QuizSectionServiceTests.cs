using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using Moq;
using AutoMapper;

namespace LearningEnglish.Tests.Application.QuizServices;

public class QuizSectionServiceTests
{
    private readonly Mock<IQuizSectionRepository> _quizSectionRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly QuizSectionService _quizSectionService;

    public QuizSectionServiceTests()
    {
        _quizSectionRepositoryMock = new Mock<IQuizSectionRepository>();
        _mapperMock = new Mock<IMapper>();

        _quizSectionService = new QuizSectionService(
            _quizSectionRepositoryMock.Object,
            _mapperMock.Object
        );
    }

    #region GetQuizSectionByIdAsync Tests

    [Fact]
    public async Task GetQuizSectionByIdAsync_WithValidId_ReturnsQuizSection()
    {
        // Arrange
        var quizSectionId = 1;
        var quizSection = new QuizSection
        {
            QuizSectionId = quizSectionId,
            Title = "Test Section",
            QuizId = 1
        };

        var quizSectionDto = new QuizSectionDto
        {
            QuizSectionId = quizSectionId,
            Title = "Test Section",
            QuizId = 1
        };

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(quizSectionId))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<QuizSectionDto>(quizSection))
            .Returns(quizSectionDto);

        // Act
        var result = await _quizSectionService.GetQuizSectionByIdAsync(quizSectionId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(quizSectionId, result.Data.QuizSectionId);
        Assert.Equal("Test Section", result.Data.Title);
        Assert.Contains("Lấy thông tin phần quiz thành công", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.GetQuizSectionByIdAsync(quizSectionId), Times.Once);
    }

    [Fact]
    public async Task GetQuizSectionByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var quizSectionId = 999;

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(quizSectionId))
            .ReturnsAsync((QuizSection?)null);

        // Act
        var result = await _quizSectionService.GetQuizSectionByIdAsync(quizSectionId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy phần quiz", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetQuizSectionsByQuizIdAsync Tests

    [Fact]
    public async Task GetQuizSectionsByQuizIdAsync_WithValidQuizId_ReturnsQuizSections()
    {
        // Arrange
        var quizId = 1;
        var quizSections = new List<QuizSection>
        {
            new QuizSection
            {
                QuizSectionId = 1,
                Title = "Section 1",
                QuizId = quizId
            },
            new QuizSection
            {
                QuizSectionId = 2,
                Title = "Section 2",
                QuizId = quizId
            }
        };

        var quizSectionDtos = new List<QuizSectionDto>
        {
            new QuizSectionDto
            {
                QuizSectionId = 1,
                Title = "Section 1",
                QuizId = quizId
            },
            new QuizSectionDto
            {
                QuizSectionId = 2,
                Title = "Section 2",
                QuizId = quizId
            }
        };

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionsByQuizIdAsync(quizId))
            .ReturnsAsync(quizSections);

        _mapperMock
            .Setup(x => x.Map<List<QuizSectionDto>>(quizSections))
            .Returns(quizSectionDtos);

        // Act
        var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains("Lấy danh sách phần quiz thành công", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.GetQuizSectionsByQuizIdAsync(quizId), Times.Once);
    }

    [Fact]
    public async Task GetQuizSectionsByQuizIdAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var quizId = 1;

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionsByQuizIdAsync(quizId))
            .ReturnsAsync(new List<QuizSection>());

        _mapperMock
            .Setup(x => x.Map<List<QuizSectionDto>>(It.IsAny<List<QuizSection>>()))
            .Returns(new List<QuizSectionDto>());

        // Act
        var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region CreateQuizSectionAsync Tests

    [Fact]
    public async Task CreateQuizSectionAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateQuizSectionDto
        {
            QuizId = 1,
            Title = "New Section"
        };

        var quiz = new Quiz
        {
            QuizId = 1,
            Title = "Test Quiz"
        };

        var quizSection = new QuizSection
        {
            QuizSectionId = 1,
            Title = dto.Title,
            QuizId = dto.QuizId
        };

        var quizSectionDto = new QuizSectionDto
        {
            QuizSectionId = 1,
            Title = dto.Title,
            QuizId = dto.QuizId
        };

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(dto.QuizId))
            .ReturnsAsync(quiz);

        _mapperMock
            .Setup(x => x.Map<QuizSection>(dto))
            .Returns(quizSection);

        _quizSectionRepositoryMock
            .Setup(x => x.CreateQuizSectionAsync(It.IsAny<QuizSection>()))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<QuizSectionDto>(It.IsAny<QuizSection>()))
            .Returns(quizSectionDto);

        // Act
        var result = await _quizSectionService.CreateQuizSectionAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Tạo phần quiz thành công", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.CreateQuizSectionAsync(It.IsAny<QuizSection>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuizSectionAsync_WithNonExistentQuiz_ReturnsError()
    {
        // Arrange
        var dto = new CreateQuizSectionDto
        {
            QuizId = 999,
            Title = "New Section"
        };

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizByIdAsync(dto.QuizId))
            .ReturnsAsync((Quiz?)null);

        // Act
        var result = await _quizSectionService.CreateQuizSectionAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Quiz không tồn tại", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.CreateQuizSectionAsync(It.IsAny<QuizSection>()), Times.Never);
    }

    #endregion

    #region UpdateQuizSectionAsync Tests

    [Fact]
    public async Task UpdateQuizSectionAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var quizSectionId = 1;
        var dto = new UpdateQuizSectionDto
        {
            Title = "Updated Section"
        };

        var existingQuizSection = new QuizSection
        {
            QuizSectionId = quizSectionId,
            Title = "Original Title",
            QuizId = 1
        };

        var updatedQuizSection = new QuizSection
        {
            QuizSectionId = quizSectionId,
            Title = dto.Title,
            QuizId = 1
        };

        var quizSectionDto = new QuizSectionDto
        {
            QuizSectionId = quizSectionId,
            Title = dto.Title,
            QuizId = 1
        };

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(quizSectionId))
            .ReturnsAsync(existingQuizSection);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateQuizSectionDto>(), It.IsAny<QuizSection>()))
            .Returns(updatedQuizSection);

        _quizSectionRepositoryMock
            .Setup(x => x.UpdateQuizSectionAsync(It.IsAny<QuizSection>()))
            .ReturnsAsync(updatedQuizSection);

        _mapperMock
            .Setup(x => x.Map<QuizSectionDto>(It.IsAny<QuizSection>()))
            .Returns(quizSectionDto);

        // Act
        var result = await _quizSectionService.UpdateQuizSectionAsync(quizSectionId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Cập nhật phần quiz thành công", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.UpdateQuizSectionAsync(It.IsAny<QuizSection>()), Times.Once);
    }

    [Fact]
    public async Task UpdateQuizSectionAsync_WithNonExistentQuizSection_ReturnsNotFound()
    {
        // Arrange
        var quizSectionId = 999;
        var dto = new UpdateQuizSectionDto
        {
            Title = "Updated Section"
        };

        _quizSectionRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(quizSectionId))
            .ReturnsAsync((QuizSection?)null);

        // Act
        var result = await _quizSectionService.UpdateQuizSectionAsync(quizSectionId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy phần quiz", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.UpdateQuizSectionAsync(It.IsAny<QuizSection>()), Times.Never);
    }

    #endregion

    #region DeleteQuizSectionAsync Tests

    [Fact]
    public async Task DeleteQuizSectionAsync_WithValidQuizSection_ReturnsSuccess()
    {
        // Arrange
        var quizSectionId = 1;

        _quizSectionRepositoryMock
            .Setup(x => x.DeleteQuizSectionAsync(quizSectionId))
            .ReturnsAsync(true);

        // Act
        var result = await _quizSectionService.DeleteQuizSectionAsync(quizSectionId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Contains("Xóa phần quiz thành công", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.DeleteQuizSectionAsync(quizSectionId), Times.Once);
    }

    [Fact]
    public async Task DeleteQuizSectionAsync_WithNonExistentQuizSection_ReturnsError()
    {
        // Arrange
        var quizSectionId = 999;

        _quizSectionRepositoryMock
            .Setup(x => x.DeleteQuizSectionAsync(quizSectionId))
            .ReturnsAsync(false);

        // Act
        var result = await _quizSectionService.DeleteQuizSectionAsync(quizSectionId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không thể xóa phần quiz", result.Message);

        _quizSectionRepositoryMock.Verify(x => x.DeleteQuizSectionAsync(quizSectionId), Times.Once);
    }

    #endregion
}

