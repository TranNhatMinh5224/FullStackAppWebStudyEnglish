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

namespace LearningEnglish.Tests.Application.EssayServices;

public class EssayServiceTests
{
    private readonly Mock<IEssayRepository> _essayRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<EssayService>> _loggerMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly EssayService _essayService;

    public EssayServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _essayRepositoryMock = new Mock<IEssayRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<EssayService>>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _essayService = new EssayService(
            _essayRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetEssayByIdAsync Tests

    [Fact]
    public async Task GetEssayByIdAsync_WithValidId_ReturnsEssay()
    {
        // Arrange
        var essayId = 1;
        var essay = new Essay
        {
            EssayId = essayId,
            Title = "Test Essay",
            AssessmentId = 1,
            Type = AssessmentType.Essay,
            AudioKey = "essays/audios/audio-123",
            ImageKey = "essays/images/image-123"
        };

        var essayDto = new EssayDto
        {
            EssayId = essayId,
            Title = "Test Essay",
            AssessmentId = 1,
            AudioUrl = "essays/audios/audio-123",
            ImageUrl = "essays/images/image-123"
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(essay);

        _mapperMock
            .Setup(x => x.Map<EssayDto>(essay))
            .Returns(essayDto);

        // Act
        var result = await _essayService.GetEssayByIdAsync(essayId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(essayId, result.Data.EssayId);
        Assert.NotNull(result.Data.AudioUrl); // Should be built from AudioKey
        Assert.NotNull(result.Data.ImageUrl); // Should be built from ImageKey

        _essayRepositoryMock.Verify(x => x.GetEssayByIdWithDetailsAsync(essayId), Times.Once);
    }

    [Fact]
    public async Task GetEssayByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var essayId = 999;

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync((Essay?)null);

        // Act
        var result = await _essayService.GetEssayByIdAsync(essayId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Essay", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetEssaysByAssessmentIdAsync Tests

    [Fact]
    public async Task GetEssaysByAssessmentIdAsync_WithValidAssessmentId_ReturnsEssays()
    {
        // Arrange
        var assessmentId = 1;
        var essays = new List<Essay>
        {
            new Essay
            {
                EssayId = 1,
                Title = "Essay 1",
                AssessmentId = assessmentId
            },
            new Essay
            {
                EssayId = 2,
                Title = "Essay 2",
                AssessmentId = assessmentId
            }
        };

        var essayDtos = new List<EssayDto>
        {
            new EssayDto
            {
                EssayId = 1,
                Title = "Essay 1",
                AssessmentId = assessmentId
            },
            new EssayDto
            {
                EssayId = 2,
                Title = "Essay 2",
                AssessmentId = assessmentId
            }
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssaysByAssessmentIdAsync(assessmentId))
            .ReturnsAsync(essays);

        _mapperMock
            .Setup(x => x.Map<List<EssayDto>>(essays))
            .Returns(essayDtos);

        // Act
        var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        _essayRepositoryMock.Verify(x => x.GetEssaysByAssessmentIdAsync(assessmentId), Times.Once);
    }

    #endregion

    #region CreateEssayAsync Tests

    [Fact]
    public async Task CreateEssayAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateEssayDto
        {
            AssessmentId = 1,
            Title = "New Essay",
            Description = "Test Description"
        };

        var essay = new Essay
        {
            EssayId = 1,
            Title = dto.Title,
            Description = dto.Description,
            AssessmentId = dto.AssessmentId,
            Type = AssessmentType.Essay
        };

        var essayDto = new EssayDto
        {
            EssayId = 1,
            Title = dto.Title,
            Description = dto.Description,
            AssessmentId = dto.AssessmentId
        };

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(true);

        _essayRepositoryMock
            .Setup(x => x.CreateEssayAsync(It.IsAny<Essay>()))
            .ReturnsAsync(essay);

        _mapperMock
            .Setup(x => x.Map<EssayDto>(It.IsAny<Essay>()))
            .Returns(essayDto);

        // Act
        var result = await _essayService.CreateEssayAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Tạo Essay thành công", result.Message);

        _essayRepositoryMock.Verify(x => x.CreateEssayAsync(It.IsAny<Essay>()), Times.Once);
    }

    [Fact]
    public async Task CreateEssayAsync_WithNonExistentAssessment_ReturnsNotFound()
    {
        // Arrange
        var dto = new CreateEssayDto
        {
            AssessmentId = 999,
            Title = "New Essay"
        };

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _essayService.CreateEssayAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Assessment không tồn tại", result.Message);

        _essayRepositoryMock.Verify(x => x.CreateEssayAsync(It.IsAny<Essay>()), Times.Never);
    }

    [Fact]
    public async Task CreateEssayAsync_AsTeacherWithOwnAssessment_ReturnsSuccess()
    {
        // Arrange
        var teacherId = 1;
        var dto = new CreateEssayDto
        {
            AssessmentId = 1,
            Title = "New Essay"
        };

        var essay = new Essay
        {
            EssayId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId
        };

        var essayDto = new EssayDto
        {
            EssayId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId
        };

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(true);

        _essayRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, dto.AssessmentId))
            .ReturnsAsync(true);

        _essayRepositoryMock
            .Setup(x => x.CreateEssayAsync(It.IsAny<Essay>()))
            .ReturnsAsync(essay);

        _mapperMock
            .Setup(x => x.Map<EssayDto>(It.IsAny<Essay>()))
            .Returns(essayDto);

        // Act
        var result = await _essayService.CreateEssayAsync(dto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateEssayAsync_AsTeacherWithWrongAssessment_ReturnsForbidden()
    {
        // Arrange
        var teacherId = 1;
        var dto = new CreateEssayDto
        {
            AssessmentId = 1,
            Title = "New Essay"
        };

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(true);

        _essayRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, dto.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _essayService.CreateEssayAsync(dto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền tạo Essay", result.Message);

        _essayRepositoryMock.Verify(x => x.CreateEssayAsync(It.IsAny<Essay>()), Times.Never);
    }

    [Fact]
    public async Task CreateEssayAsync_WithAudioAndImage_CommitsFiles()
    {
        // Arrange
        var dto = new CreateEssayDto
        {
            AssessmentId = 1,
            Title = "New Essay",
            AudioTempKey = "temp/audio-123",
            ImageTempKey = "temp/image-123"
        };

        var essay = new Essay
        {
            EssayId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId,
            AudioKey = "essays/audios/audio-123",
            ImageKey = "essays/images/image-123"
        };

        var essayDto = new EssayDto
        {
            EssayId = 1,
            Title = dto.Title,
            AssessmentId = dto.AssessmentId
        };

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(true);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.AudioTempKey!, "essays", "audios"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "essays/audios/audio-123"
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey!, "essays", "images"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "essays/images/image-123"
            });

        _essayRepositoryMock
            .Setup(x => x.CreateEssayAsync(It.Is<Essay>(e => 
                e.AudioKey == "essays/audios/audio-123" &&
                e.ImageKey == "essays/images/image-123")))
            .ReturnsAsync(essay);

        _mapperMock
            .Setup(x => x.Map<EssayDto>(It.IsAny<Essay>()))
            .Returns(essayDto);

        // Act
        var result = await _essayService.CreateEssayAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.AudioTempKey!, "essays", "audios"), Times.Once);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.ImageTempKey!, "essays", "images"), Times.Once);
    }

    [Fact]
    public async Task CreateEssayAsync_WithAudioCommitFailure_ReturnsError()
    {
        // Arrange
        var dto = new CreateEssayDto
        {
            AssessmentId = 1,
            Title = "New Essay",
            AudioTempKey = "temp/audio-123"
        };

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(true);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.AudioTempKey!, "essays", "audios"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit"
            });

        // Act
        var result = await _essayService.CreateEssayAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Không thể lưu file audio", result.Message);

        _essayRepositoryMock.Verify(x => x.CreateEssayAsync(It.IsAny<Essay>()), Times.Never);
    }

    [Fact]
    public async Task CreateEssayAsync_WithDatabaseError_RollsBackFiles()
    {
        // Arrange
        var dto = new CreateEssayDto
        {
            AssessmentId = 1,
            Title = "New Essay",
            AudioTempKey = "temp/audio-123",
            ImageTempKey = "temp/image-123"
        };

        var committedAudioKey = "essays/audios/audio-123";
        var committedImageKey = "essays/images/image-123";

        _essayRepositoryMock
            .Setup(x => x.AssessmentExistsAsync(dto.AssessmentId))
            .ReturnsAsync(true);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.AudioTempKey!, "essays", "audios"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedAudioKey
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey!, "essays", "images"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedImageKey
            });

        _essayRepositoryMock
            .Setup(x => x.CreateEssayAsync(It.IsAny<Essay>()))
            .ThrowsAsync(new Exception("Database error"));

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _essayService.CreateEssayAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi database", result.Message);

        // Should rollback both files
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedAudioKey, "essays"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedImageKey, "essays"), Times.Once);
    }

    #endregion

    #region UpdateEssayAsync Tests

    [Fact]
    public async Task UpdateEssayAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var essayId = 1;
        var dto = new UpdateEssayDto
        {
            Title = "Updated Essay"
        };

        var existingEssay = new Essay
        {
            EssayId = essayId,
            Title = "Original Essay",
            AssessmentId = 1
        };

        var updatedEssay = new Essay
        {
            EssayId = essayId,
            Title = dto.Title,
            AssessmentId = 1
        };

        var essayDto = new EssayDto
        {
            EssayId = essayId,
            Title = dto.Title,
            AssessmentId = 1
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(existingEssay);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateEssayDto>(), It.IsAny<Essay>()))
            .Callback<UpdateEssayDto, Essay>((d, e) =>
            {
                if (!string.IsNullOrEmpty(d.Title))
                    e.Title = d.Title;
            });

        _essayRepositoryMock
            .Setup(x => x.UpdateEssayAsync(It.IsAny<Essay>()))
            .ReturnsAsync(It.IsAny<Essay>());

        _mapperMock
            .Setup(x => x.Map<EssayDto>(It.IsAny<Essay>()))
            .Returns(essayDto);

        // Act
        var result = await _essayService.UpdateEssayAsync(essayId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Cập nhật Essay thành công", result.Message);

        _essayRepositoryMock.Verify(x => x.UpdateEssayAsync(It.IsAny<Essay>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEssayAsync_WithNonExistentEssay_ReturnsNotFound()
    {
        // Arrange
        var essayId = 999;
        var dto = new UpdateEssayDto
        {
            Title = "Updated Essay"
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync((Essay?)null);

        // Act
        var result = await _essayService.UpdateEssayAsync(essayId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Essay", result.Message);

        _essayRepositoryMock.Verify(x => x.UpdateEssayAsync(It.IsAny<Essay>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEssayAsync_AsTeacherWithOwnAssessment_ReturnsSuccess()
    {
        // Arrange
        var essayId = 1;
        var teacherId = 1;
        var dto = new UpdateEssayDto
        {
            Title = "Updated Essay"
        };

        var existingEssay = new Essay
        {
            EssayId = essayId,
            Title = "Original Essay",
            AssessmentId = 1
        };

        var updatedEssay = new Essay
        {
            EssayId = essayId,
            Title = dto.Title,
            AssessmentId = 1
        };

        var essayDto = new EssayDto
        {
            EssayId = essayId,
            Title = dto.Title
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(existingEssay);

        _essayRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, existingEssay.AssessmentId))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateEssayDto>(), It.IsAny<Essay>()))
            .Returns(updatedEssay);

        _essayRepositoryMock
            .Setup(x => x.UpdateEssayAsync(It.IsAny<Essay>()))
            .ReturnsAsync(It.IsAny<Essay>());

        _mapperMock
            .Setup(x => x.Map<EssayDto>(It.IsAny<Essay>()))
            .Returns(essayDto);

        // Act
        var result = await _essayService.UpdateEssayAsync(essayId, dto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateEssayAsync_AsTeacherWithWrongAssessment_ReturnsForbidden()
    {
        // Arrange
        var essayId = 1;
        var teacherId = 1;
        var dto = new UpdateEssayDto
        {
            Title = "Updated Essay"
        };

        var existingEssay = new Essay
        {
            EssayId = essayId,
            Title = "Original Essay",
            AssessmentId = 1
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(existingEssay);

        _essayRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, existingEssay.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _essayService.UpdateEssayAsync(essayId, dto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền cập nhật Essay", result.Message);

        _essayRepositoryMock.Verify(x => x.UpdateEssayAsync(It.IsAny<Essay>()), Times.Never);
    }

    #endregion

    #region DeleteEssayAsync Tests

    [Fact]
    public async Task DeleteEssayAsync_WithValidEssay_ReturnsSuccess()
    {
        // Arrange
        var essayId = 1;

        var essay = new Essay
        {
            EssayId = essayId,
            Title = "Test Essay",
            AssessmentId = 1,
            AudioKey = "essays/audios/audio-123",
            ImageKey = "essays/images/image-123"
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(essay);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        _essayRepositoryMock
            .Setup(x => x.DeleteEssayAsync(essayId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _essayService.DeleteEssayAsync(essayId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Xóa Essay thành công", result.Message);

        // Should delete both audio and image files
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(essay.AudioKey!, "essays"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(essay.ImageKey!, "essays"), Times.Once);
        _essayRepositoryMock.Verify(x => x.DeleteEssayAsync(essayId), Times.Once);
    }

    [Fact]
    public async Task DeleteEssayAsync_WithNonExistentEssay_ReturnsNotFound()
    {
        // Arrange
        var essayId = 999;

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync((Essay?)null);

        // Act
        var result = await _essayService.DeleteEssayAsync(essayId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Essay", result.Message);

        _essayRepositoryMock.Verify(x => x.DeleteEssayAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteEssayAsync_AsTeacherWithOwnAssessment_ReturnsSuccess()
    {
        // Arrange
        var essayId = 1;
        var teacherId = 1;

        var essay = new Essay
        {
            EssayId = essayId,
            Title = "Test Essay",
            AssessmentId = 1
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(essay);

        _essayRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, essay.AssessmentId))
            .ReturnsAsync(true);

        _essayRepositoryMock
            .Setup(x => x.DeleteEssayAsync(essayId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _essayService.DeleteEssayAsync(essayId, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteEssayAsync_AsTeacherWithWrongAssessment_ReturnsForbidden()
    {
        // Arrange
        var essayId = 1;
        var teacherId = 1;

        var essay = new Essay
        {
            EssayId = essayId,
            Title = "Test Essay",
            AssessmentId = 1
        };

        _essayRepositoryMock
            .Setup(x => x.GetEssayByIdWithDetailsAsync(essayId))
            .ReturnsAsync(essay);

        _essayRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfAssessmentAsync(teacherId, essay.AssessmentId))
            .ReturnsAsync(false);

        // Act
        var result = await _essayService.DeleteEssayAsync(essayId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền xóa Essay", result.Message);

        _essayRepositoryMock.Verify(x => x.DeleteEssayAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}

