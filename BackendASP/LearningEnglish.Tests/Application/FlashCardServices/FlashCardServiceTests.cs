using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application;

public class FlashCardServiceTests
{
    private readonly Mock<IFlashCardRepository> _flashCardRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<FlashCardService>> _loggerMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly FlashCardService _flashCardService;

    public FlashCardServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _flashCardRepositoryMock = new Mock<IFlashCardRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<FlashCardService>>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _flashCardService = new FlashCardService(
            _flashCardRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _moduleRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetFlashCardByIdAsync Tests

    [Fact]
    public async Task GetFlashCardByIdAsync_WithValidId_ReturnsFlashCard()
    {
        // Arrange
        var flashCardId = 1;
        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Beautiful",
            Meaning = "Đẹp",
            ModuleId = 1,
            ImageKey = "flashcards/real/image-123",
            AudioKey = "flashcard-audio/real/audio-123"
        };

        var flashCardDto = new FlashCardDto
        {
            FlashCardId = flashCardId,
            Word = "Beautiful",
            Meaning = "Đẹp",
            ModuleId = 1,
            ImageUrl = "flashcards/real/image-123",
            AudioUrl = "flashcard-audio/real/audio-123"
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(flashCardId))
            .ReturnsAsync(flashCard);

        _mapperMock
            .Setup(x => x.Map<FlashCardDto>(flashCard))
            .Returns(flashCardDto);

        // Act
        var result = await _flashCardService.GetFlashCardByIdAsync(flashCardId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(flashCardId, result.Data.FlashCardId);
        Assert.NotNull(result.Data.ImageUrl); // Should be built from ImageKey
        Assert.NotNull(result.Data.AudioUrl); // Should be built from AudioKey

        _flashCardRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(flashCardId), Times.Once);
    }

    [Fact]
    public async Task GetFlashCardByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var flashCardId = 999;

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(flashCardId))
            .ReturnsAsync((FlashCard?)null);

        // Act
        var result = await _flashCardService.GetFlashCardByIdAsync(flashCardId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy FlashCard", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetFlashCardByIdAsync_WithNullImageAndAudio_DoesNotBuildUrls()
    {
        // Arrange
        var flashCardId = 1;
        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Test",
            Meaning = "Test",
            ModuleId = 1,
            ImageKey = null,
            AudioKey = null
        };

        var flashCardDto = new FlashCardDto
        {
            FlashCardId = flashCardId,
            Word = "Test",
            Meaning = "Test",
            ModuleId = 1
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(flashCardId))
            .ReturnsAsync(flashCard);

        _mapperMock
            .Setup(x => x.Map<FlashCardDto>(flashCard))
            .Returns(flashCardDto);

        // Act
        var result = await _flashCardService.GetFlashCardByIdAsync(flashCardId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.ImageUrl);
        Assert.Null(result.Data.AudioUrl);
    }

    #endregion

    #region GetFlashCardsByModuleIdAsync Tests

    [Fact]
    public async Task GetFlashCardsByModuleIdAsync_WithValidModuleId_ReturnsFlashCards()
    {
        // Arrange
        var moduleId = 1;
        var flashCards = new List<FlashCard>
        {
            new FlashCard
            {
                FlashCardId = 1,
                Word = "Beautiful",
                Meaning = "Đẹp",
                ModuleId = moduleId,
                ImageKey = "flashcards/real/image-1"
            },
            new FlashCard
            {
                FlashCardId = 2,
                Word = "Ugly",
                Meaning = "Xấu",
                ModuleId = moduleId,
                AudioKey = "flashcard-audio/real/audio-2"
            }
        };

        var flashCardDtos = new List<ListFlashCardDto>
        {
            new ListFlashCardDto
            {
                FlashCardId = 1,
                Word = "Beautiful",
                Meaning = "Đẹp",
                ModuleId = moduleId,
                ImageUrl = "flashcards/real/image-1"
            },
            new ListFlashCardDto
            {
                FlashCardId = 2,
                Word = "Ugly",
                Meaning = "Xấu",
                ModuleId = moduleId,
                AudioUrl = "flashcard-audio/real/audio-2"
            }
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdWithDetailsAsync(moduleId))
            .ReturnsAsync(flashCards);

        _mapperMock
            .Setup(x => x.Map<List<ListFlashCardDto>>(flashCards))
            .Returns(flashCardDtos);

        // Act
        var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains("Lấy danh sách 2 FlashCard thành công", result.Message);

        _flashCardRepositoryMock.Verify(x => x.GetByModuleIdWithDetailsAsync(moduleId), Times.Once);
    }

    [Fact]
    public async Task GetFlashCardsByModuleIdAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var moduleId = 1;

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdWithDetailsAsync(moduleId))
            .ReturnsAsync(new List<FlashCard>());

        _mapperMock
            .Setup(x => x.Map<List<ListFlashCardDto>>(It.IsAny<List<FlashCard>>()))
            .Returns(new List<ListFlashCardDto>());

        // Act
        var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region GetFlashCardsByModuleIdPaginatedAsync Tests

    [Fact]
    public async Task GetFlashCardsByModuleIdPaginatedAsync_WithValidRequest_ReturnsPagedResult()
    {
        // Arrange
        var moduleId = 1;
        var request = new PageRequest { PageNumber = 1, PageSize = 2 };

        var allFlashCards = new List<FlashCard>
        {
            new FlashCard { FlashCardId = 1, Word = "Word1", ModuleId = moduleId },
            new FlashCard { FlashCardId = 2, Word = "Word2", ModuleId = moduleId },
            new FlashCard { FlashCardId = 3, Word = "Word3", ModuleId = moduleId }
        };

        var flashCardDtos = new List<ListFlashCardDto>
        {
            new ListFlashCardDto { FlashCardId = 1, Word = "Word1", ModuleId = moduleId },
            new ListFlashCardDto { FlashCardId = 2, Word = "Word2", ModuleId = moduleId }
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdWithDetailsAsync(moduleId))
            .ReturnsAsync(allFlashCards);

        _mapperMock
            .Setup(x => x.Map<List<ListFlashCardDto>>(It.IsAny<List<FlashCard>>()))
            .Returns(flashCardDtos);

        // Act
        var result = await _flashCardService.GetFlashCardsByModuleIdPaginatedAsync(moduleId, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.Equal(3, result.Data.TotalCount);
        Assert.Equal(1, result.Data.PageNumber);
        Assert.Equal(2, result.Data.PageSize);

        _flashCardRepositoryMock.Verify(x => x.GetByModuleIdWithDetailsAsync(moduleId), Times.Once);
    }

    [Fact]
    public async Task GetFlashCardsByModuleIdPaginatedAsync_WithEmptyList_ReturnsEmptyPagedResult()
    {
        // Arrange
        var moduleId = 1;
        var request = new PageRequest { PageNumber = 1, PageSize = 10 };

        _flashCardRepositoryMock
            .Setup(x => x.GetByModuleIdWithDetailsAsync(moduleId))
            .ReturnsAsync(new List<FlashCard>());

        // Act
        var result = await _flashCardService.GetFlashCardsByModuleIdPaginatedAsync(moduleId, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data.Items);
        Assert.Equal(0, result.Data.TotalCount);
        Assert.Contains("Module không có flashcard nào", result.Message);
    }

    #endregion

    #region CreateFlashCardAsync Tests

    [Fact]
    public async Task CreateFlashCardAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateFlashCardDto
        {
            ModuleId = 1,
            Word = "Beautiful",
            Meaning = "Đẹp",
            Pronunciation = "/ˈbjuːtɪfl/"
        };

        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = dto.Word,
            Meaning = dto.Meaning,
            Pronunciation = dto.Pronunciation,
            ModuleId = dto.ModuleId
        };

        var flashCardDto = new FlashCardDto
        {
            FlashCardId = 1,
            Word = dto.Word,
            Meaning = dto.Meaning,
            Pronunciation = dto.Pronunciation,
            ModuleId = dto.ModuleId
        };

        _mapperMock
            .Setup(x => x.Map<FlashCard>(dto))
            .Returns(flashCard);

        _flashCardRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<FlashCard>()))
            .ReturnsAsync(flashCard);

        _mapperMock
            .Setup(x => x.Map<FlashCardDto>(flashCard))
            .Returns(flashCardDto);

        // Act
        var result = await _flashCardService.CreateFlashCardAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Word, result.Data.Word);
        Assert.Contains("Tạo FlashCard thành công", result.Message);

        _flashCardRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FlashCard>()), Times.Once);
    }

    [Fact]
    public async Task CreateFlashCardAsync_WithImageAndAudio_CommitsFiles()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateFlashCardDto
        {
            ModuleId = 1,
            Word = "Beautiful",
            Meaning = "Đẹp",
            ImageTempKey = "temp/image-123",
            AudioTempKey = "temp/audio-123"
        };

        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = dto.Word,
            Meaning = dto.Meaning,
            ModuleId = dto.ModuleId,
            ImageKey = "flashcards/real/image-123",
            AudioKey = "flashcard-audio/real/audio-123"
        };

        var flashCardDto = new FlashCardDto
        {
            FlashCardId = 1,
            Word = dto.Word,
            Meaning = dto.Meaning,
            ModuleId = dto.ModuleId
        };

        _mapperMock
            .Setup(x => x.Map<FlashCard>(dto))
            .Returns(flashCard);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "flashcards", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "flashcards/real/image-123"
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.AudioTempKey, "flashcard-audio", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "flashcard-audio/real/audio-123"
            });

        _flashCardRepositoryMock
            .Setup(x => x.CreateAsync(It.Is<FlashCard>(fc =>
                fc.ImageKey == "flashcards/real/image-123" &&
                fc.AudioKey == "flashcard-audio/real/audio-123")))
            .ReturnsAsync(flashCard);

        _mapperMock
            .Setup(x => x.Map<FlashCardDto>(flashCard))
            .Returns(flashCardDto);

        // Act
        var result = await _flashCardService.CreateFlashCardAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.ImageTempKey, "flashcards", "real"), Times.Once);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.AudioTempKey, "flashcard-audio", "real"), Times.Once);
    }

    [Fact]
    public async Task CreateFlashCardAsync_WithImageCommitFailure_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateFlashCardDto
        {
            ModuleId = 1,
            Word = "Beautiful",
            Meaning = "Đẹp",
            ImageTempKey = "temp/image-123"
        };

        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = dto.Word,
            Meaning = dto.Meaning,
            ModuleId = dto.ModuleId
        };

        _mapperMock
            .Setup(x => x.Map<FlashCard>(dto))
            .Returns(flashCard);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "flashcards", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit file"
            });

        // Act
        var result = await _flashCardService.CreateFlashCardAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không thể lưu ảnh", result.Message);

        _flashCardRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<FlashCard>()), Times.Never);
    }

    [Fact]
    public async Task CreateFlashCardAsync_WithDatabaseError_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateFlashCardDto
        {
            ModuleId = 1,
            Word = "Beautiful",
            Meaning = "Đẹp"
        };

        var flashCard = new FlashCard
        {
            FlashCardId = 1,
            Word = dto.Word,
            Meaning = dto.Meaning,
            ModuleId = dto.ModuleId
        };

        _mapperMock
            .Setup(x => x.Map<FlashCard>(dto))
            .Returns(flashCard);

        _flashCardRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<FlashCard>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _flashCardService.CreateFlashCardAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Lỗi database", result.Message);
    }

    #endregion

    #region UpdateFlashCardAsync Tests

    [Fact]
    public async Task UpdateFlashCardAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var flashCardId = 1;
        var userId = 1;
        var userRole = "Admin";
        var dto = new UpdateFlashCardDto
        {
            Word = "Updated Word",
            Meaning = "Updated Meaning"
        };

        var existingFlashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Original Word",
            Meaning = "Original Meaning",
            ModuleId = 1
        };

        var updatedFlashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = dto.Word,
            Meaning = dto.Meaning,
            ModuleId = 1
        };

        var flashCardDto = new FlashCardDto
        {
            FlashCardId = flashCardId,
            Word = dto.Word,
            Meaning = dto.Meaning,
            ModuleId = 1
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(existingFlashCard);

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(flashCardId))
            .ReturnsAsync(existingFlashCard);

        _flashCardRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<FlashCard>()))
            .ReturnsAsync(updatedFlashCard);

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(flashCardId))
            .ReturnsAsync(updatedFlashCard);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateFlashCardDto>(), It.IsAny<FlashCard>()))
            .Returns(updatedFlashCard);

        _mapperMock
            .Setup(x => x.Map<FlashCardDto>(updatedFlashCard))
            .Returns(flashCardDto);

        // Act
        var result = await _flashCardService.UpdateFlashCardAsync(flashCardId, dto, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Word, result.Data.Word);

        _flashCardRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FlashCard>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFlashCardAsync_AsTeacherWithOwnCourse_ReturnsSuccess()
    {
        // Arrange
        var flashCardId = 1;
        var userId = 1;
        var userRole = "Teacher";
        var dto = new UpdateFlashCardDto { Word = "Updated Word" };

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.Teacher,
            TeacherId = userId
        };

        var existingFlashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Original Word",
            ModuleId = 1,
            Module = module
        };

        existingFlashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        var updatedFlashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = dto.Word,
            ModuleId = 1
        };

        var flashCardDto = new FlashCardDto
        {
            FlashCardId = flashCardId,
            Word = dto.Word
        };

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(existingFlashCard);

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(flashCardId))
            .ReturnsAsync(existingFlashCard);

        _flashCardRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<FlashCard>()))
            .ReturnsAsync(updatedFlashCard);

        _flashCardRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(flashCardId))
            .ReturnsAsync(updatedFlashCard);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateFlashCardDto>(), It.IsAny<FlashCard>()))
            .Returns(updatedFlashCard);

        _mapperMock
            .Setup(x => x.Map<FlashCardDto>(updatedFlashCard))
            .Returns(flashCardDto);

        // Act
        var result = await _flashCardService.UpdateFlashCardAsync(flashCardId, dto, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateFlashCardAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var flashCardId = 1;
        var userId = 1;
        var ownerId = 2;
        var userRole = "Teacher";
        var dto = new UpdateFlashCardDto { Word = "Updated Word" };

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        var existingFlashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Original Word",
            ModuleId = 1,
            Module = module
        };

        existingFlashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(existingFlashCard);

        // Act
        var result = await _flashCardService.UpdateFlashCardAsync(flashCardId, dto, userId, userRole);

        // Assert
        Assert.False(result.Success);
        // StatusCode might not be set explicitly, just check message
        Assert.Contains("Bạn không có quyền cập nhật FlashCard này", result.Message);

        _flashCardRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FlashCard>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFlashCardAsync_WithNonExistentFlashCard_ReturnsNotFound()
    {
        // Arrange
        var flashCardId = 999;
        var userId = 1;
        var userRole = "Admin";
        var dto = new UpdateFlashCardDto { Word = "Updated Word" };

        // Admin doesn't check permission, goes directly to UpdateFlashCardCoreAsync
        // UpdateFlashCardCoreAsync calls GetByIdAsync
        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(flashCardId))
            .ReturnsAsync((FlashCard?)null);

        // Act
        var result = await _flashCardService.UpdateFlashCardAsync(flashCardId, dto, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy FlashCard", result.Message);

        _flashCardRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<FlashCard>()), Times.Never);
    }

    #endregion

    #region DeleteFlashCardAsync Tests

    [Fact]
    public async Task DeleteFlashCardAsync_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var flashCardId = 1;
        var userId = 1;
        var userRole = "Admin";

        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Test",
            Meaning = "Test",
            ModuleId = 1,
            ImageKey = "flashcards/real/image-123",
            AudioKey = "flashcard-audio/real/audio-123"
        };

        // Admin doesn't check permission, goes directly to GetByIdAsync
        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(flashCardId))
            .ReturnsAsync(flashCard);

        _flashCardRepositoryMock
            .Setup(x => x.DeleteAsync(flashCardId))
            .ReturnsAsync(true);

        // Act
        var result = await _flashCardService.DeleteFlashCardAsync(flashCardId, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Contains("Xóa FlashCard thành công", result.Message);

        _flashCardRepositoryMock.Verify(x => x.DeleteAsync(flashCardId), Times.Once);
    }

    [Fact]
    public async Task DeleteFlashCardAsync_AsTeacherWithOwnCourse_ReturnsSuccess()
    {
        // Arrange
        var flashCardId = 1;
        var userId = 1;
        var userRole = "Teacher";

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.Teacher,
            TeacherId = userId
        };

        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Test",
            ModuleId = 1,
            Module = module
        };

        flashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(flashCard);

        // After permission check, DeleteFlashCardAsync calls GetByIdAsync
        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(flashCardId))
            .ReturnsAsync(flashCard);

        _flashCardRepositoryMock
            .Setup(x => x.DeleteAsync(flashCardId))
            .ReturnsAsync(true);

        // Act
        var result = await _flashCardService.DeleteFlashCardAsync(flashCardId, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteFlashCardAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var flashCardId = 1;
        var userId = 1;
        var ownerId = 2;
        var userRole = "Teacher";

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            Word = "Test",
            ModuleId = 1,
            Module = module
        };

        flashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(flashCard);

        // CheckTeacherFlashCardPermission will return false, so it returns 403 before calling GetByIdAsync

        // Act
        var result = await _flashCardService.DeleteFlashCardAsync(flashCardId, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Bạn không có quyền xóa FlashCard này", result.Message);

        _flashCardRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteFlashCardAsync_WithNonExistentFlashCard_ReturnsNotFound()
    {
        // Arrange
        var flashCardId = 999;
        var userId = 1;
        var userRole = "Admin";

        // Admin doesn't check permission, goes directly to GetByIdAsync
        _flashCardRepositoryMock
            .Setup(x => x.GetByIdAsync(flashCardId))
            .ReturnsAsync((FlashCard?)null);

        // Act
        var result = await _flashCardService.DeleteFlashCardAsync(flashCardId, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy FlashCard", result.Message);

        _flashCardRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region CheckTeacherFlashCardPermission Tests

    [Fact]
    public async Task CheckTeacherFlashCardPermission_WithOwnCourse_ReturnsTrue()
    {
        // Arrange
        var flashCardId = 1;
        var teacherId = 1;

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.Teacher,
            TeacherId = teacherId
        };

        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            ModuleId = 1,
            Module = module
        };

        flashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(flashCard);

        // Act
        var result = await _flashCardService.CheckTeacherFlashCardPermission(flashCardId, teacherId);

        // Assert
        Assert.True(result);

        _flashCardRepositoryMock.Verify(x => x.GetFlashCardWithModuleCourseAsync(flashCardId), Times.Once);
    }

    [Fact]
    public async Task CheckTeacherFlashCardPermission_WithWrongCourse_ReturnsFalse()
    {
        // Arrange
        var flashCardId = 1;
        var teacherId = 1;
        var ownerId = 2;

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            ModuleId = 1,
            Module = module
        };

        flashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(flashCard);

        // Act
        var result = await _flashCardService.CheckTeacherFlashCardPermission(flashCardId, teacherId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckTeacherFlashCardPermission_WithSystemCourse_ReturnsFalse()
    {
        // Arrange
        var flashCardId = 1;
        var teacherId = 1;

        var module = new Module
        {
            ModuleId = 1,
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Type = CourseType.System // System course
        };

        var flashCard = new FlashCard
        {
            FlashCardId = flashCardId,
            ModuleId = 1,
            Module = module
        };

        flashCard.Module!.Lesson = lesson;
        lesson.Course = course;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync(flashCard);

        // Act
        var result = await _flashCardService.CheckTeacherFlashCardPermission(flashCardId, teacherId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckTeacherFlashCardPermission_WithNonExistentFlashCard_ReturnsFalse()
    {
        // Arrange
        var flashCardId = 999;
        var teacherId = 1;

        _flashCardRepositoryMock
            .Setup(x => x.GetFlashCardWithModuleCourseAsync(flashCardId))
            .ReturnsAsync((FlashCard?)null);

        // Act
        var result = await _flashCardService.CheckTeacherFlashCardPermission(flashCardId, teacherId);

        // Assert
        Assert.False(result);
    }

    #endregion
}

