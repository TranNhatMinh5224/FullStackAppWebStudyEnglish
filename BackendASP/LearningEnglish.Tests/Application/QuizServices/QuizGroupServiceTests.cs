using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application.QuizServices;

public class QuizGroupServiceTests
{
    private readonly Mock<IQuizGroupRepository> _quizGroupRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly QuizGroupService _quizGroupService;

    public QuizGroupServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _quizGroupRepositoryMock = new Mock<IQuizGroupRepository>();
        _mapperMock = new Mock<IMapper>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _quizGroupService = new QuizGroupService(
            _quizGroupRepositoryMock.Object,
            _mapperMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetQuizGroupByIdAsync Tests

    [Fact]
    public async Task GetQuizGroupByIdAsync_WithValidId_ReturnsQuizGroup()
    {
        // Arrange
        var quizGroupId = 1;
        var quizGroup = new QuizGroup
        {
            QuizGroupId = quizGroupId,
            Title = "Test Quiz Group",
            QuizSectionId = 1,
            ImgKey = "quizgroups/real/img-123",
            VideoKey = "quizgroups/real/video-123"
        };

        var quizGroupDto = new QuizGroupDto
        {
            QuizGroupId = quizGroupId,
            Title = "Test Quiz Group",
            QuizSectionId = 1,
            ImgUrl = "quizgroups/real/img-123",
            VideoUrl = "quizgroups/real/video-123"
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(quizGroupId))
            .ReturnsAsync(quizGroup);

        _mapperMock
            .Setup(x => x.Map<QuizGroupDto>(quizGroup))
            .Returns(quizGroupDto);

        // Act
        var result = await _quizGroupService.GetQuizGroupByIdAsync(quizGroupId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(quizGroupId, result.Data.QuizGroupId);
        Assert.NotNull(result.Data.ImgUrl); // Should be built from ImgKey
        Assert.NotNull(result.Data.VideoUrl); // Should be built from VideoKey

        _quizGroupRepositoryMock.Verify(x => x.GetQuizGroupByIdAsync(quizGroupId), Times.Once);
    }

    [Fact]
    public async Task GetQuizGroupByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var quizGroupId = 999;

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(quizGroupId))
            .ReturnsAsync((QuizGroup?)null);

        // Act
        var result = await _quizGroupService.GetQuizGroupByIdAsync(quizGroupId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy quiz group", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetQuizGroupsByQuizSectionIdAsync Tests

    [Fact]
    public async Task GetQuizGroupsByQuizSectionIdAsync_WithValidSectionId_ReturnsQuizGroups()
    {
        // Arrange
        var quizSectionId = 1;
        var quizGroups = new List<QuizGroup>
        {
            new QuizGroup
            {
                QuizGroupId = 1,
                Title = "Group 1",
                QuizSectionId = quizSectionId
            },
            new QuizGroup
            {
                QuizGroupId = 2,
                Title = "Group 2",
                QuizSectionId = quizSectionId
            }
        };

        var quizGroupDtos = new List<QuizGroupDto>
        {
            new QuizGroupDto
            {
                QuizGroupId = 1,
                Title = "Group 1",
                QuizSectionId = quizSectionId
            },
            new QuizGroupDto
            {
                QuizGroupId = 2,
                Title = "Group 2",
                QuizSectionId = quizSectionId
            }
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupsByQuizSectionIdAsync(quizSectionId))
            .ReturnsAsync(quizGroups);

        _mapperMock
            .Setup(x => x.Map<List<QuizGroupDto>>(quizGroups))
            .Returns(quizGroupDtos);

        // Act
        var result = await _quizGroupService.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        _quizGroupRepositoryMock.Verify(x => x.GetQuizGroupsByQuizSectionIdAsync(quizSectionId), Times.Once);
    }

    #endregion

    #region CreateQuizGroupAsync Tests

    [Fact]
    public async Task CreateQuizGroupAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateQuizGroupDto
        {
            QuizSectionId = 1,
            Title = "New Quiz Group"
        };

        var quizSection = new QuizSection
        {
            QuizSectionId = 1,
            Title = "Test Section"
        };

        var quizGroup = new QuizGroup
        {
            QuizGroupId = 1,
            Title = dto.Title,
            QuizSectionId = dto.QuizSectionId
        };

        var quizGroupDto = new QuizGroupDto
        {
            QuizGroupId = 1,
            Title = dto.Title,
            QuizSectionId = dto.QuizSectionId
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<QuizGroup>(dto))
            .Returns(quizGroup);

        _quizGroupRepositoryMock
            .Setup(x => x.CreateQuizGroupAsync(It.IsAny<QuizGroup>()))
            .ReturnsAsync(quizGroup);

        _mapperMock
            .Setup(x => x.Map<QuizGroupDto>(It.IsAny<QuizGroup>()))
            .Returns(quizGroupDto);

        // Act
        var result = await _quizGroupService.CreateQuizGroupAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Tạo nhóm câu hỏi thành công", result.Message);

        _quizGroupRepositoryMock.Verify(x => x.CreateQuizGroupAsync(It.IsAny<QuizGroup>()), Times.Once);
    }

    [Fact]
    public async Task CreateQuizGroupAsync_WithNonExistentQuizSection_ReturnsError()
    {
        // Arrange
        var dto = new CreateQuizGroupDto
        {
            QuizSectionId = 999,
            Title = "New Quiz Group"
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync((QuizSection?)null);

        // Act
        var result = await _quizGroupService.CreateQuizGroupAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Quiz section không tồn tại", result.Message);

        _quizGroupRepositoryMock.Verify(x => x.CreateQuizGroupAsync(It.IsAny<QuizGroup>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuizGroupAsync_WithImageAndVideo_CommitsFiles()
    {
        // Arrange
        var dto = new CreateQuizGroupDto
        {
            QuizSectionId = 1,
            Title = "New Quiz Group",
            ImgTempKey = "temp/img-123",
            VideoTempKey = "temp/video-123"
        };

        var quizSection = new QuizSection { QuizSectionId = 1 };
        var quizGroup = new QuizGroup
        {
            QuizGroupId = 1,
            Title = dto.Title,
            QuizSectionId = dto.QuizSectionId,
            ImgKey = "quizgroups/real/img-123",
            VideoKey = "quizgroups/real/video-123"
        };

        var quizGroupDto = new QuizGroupDto
        {
            QuizGroupId = 1,
            Title = dto.Title
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<QuizGroup>(dto))
            .Returns(quizGroup);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImgTempKey!, "quizgroups", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "quizgroups/real/img-123"
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.VideoTempKey!, "quizgroups", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "quizgroups/real/video-123"
            });

        _quizGroupRepositoryMock
            .Setup(x => x.CreateQuizGroupAsync(It.IsAny<QuizGroup>()))
            .ReturnsAsync(quizGroup);

        _mapperMock
            .Setup(x => x.Map<QuizGroupDto>(It.IsAny<QuizGroup>()))
            .Returns(quizGroupDto);

        // Act
        var result = await _quizGroupService.CreateQuizGroupAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.ImgTempKey!, "quizgroups", "real"), Times.Once);
        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.VideoTempKey!, "quizgroups", "real"), Times.Once);
    }

    [Fact]
    public async Task CreateQuizGroupAsync_WithVideoCommitFailure_RollsBackImage()
    {
        // Arrange
        var dto = new CreateQuizGroupDto
        {
            QuizSectionId = 1,
            Title = "New Quiz Group",
            ImgTempKey = "temp/img-123",
            VideoTempKey = "temp/video-123"
        };

        var quizSection = new QuizSection { QuizSectionId = 1 };
        var quizGroup = new QuizGroup { QuizGroupId = 1 };

        var committedImgKey = "quizgroups/real/img-123";

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<QuizGroup>(dto))
            .Returns(quizGroup);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImgTempKey!, "quizgroups", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedImgKey
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.VideoTempKey!, "quizgroups", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit"
            });

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(committedImgKey, "quizgroups"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _quizGroupService.CreateQuizGroupAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không thể lưu video", result.Message);

        // Should rollback image
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedImgKey, "quizgroups"), Times.Once);
        _quizGroupRepositoryMock.Verify(x => x.CreateQuizGroupAsync(It.IsAny<QuizGroup>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuizGroupAsync_WithDatabaseError_RollsBackAllFiles()
    {
        // Arrange
        var dto = new CreateQuizGroupDto
        {
            QuizSectionId = 1,
            Title = "New Quiz Group",
            ImgTempKey = "temp/img-123",
            VideoTempKey = "temp/video-123"
        };

        var quizSection = new QuizSection { QuizSectionId = 1 };
        var quizGroup = new QuizGroup { QuizGroupId = 1 };

        var committedImgKey = "quizgroups/real/img-123";
        var committedVideoKey = "quizgroups/real/video-123";

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizSectionByIdAsync(dto.QuizSectionId))
            .ReturnsAsync(quizSection);

        _mapperMock
            .Setup(x => x.Map<QuizGroup>(dto))
            .Returns(quizGroup);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImgTempKey!, "quizgroups", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedImgKey
            });

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.VideoTempKey!, "quizgroups", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedVideoKey
            });

        _quizGroupRepositoryMock
            .Setup(x => x.CreateQuizGroupAsync(It.IsAny<QuizGroup>()))
            .ThrowsAsync(new Exception("Database error"));

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), "quizgroups"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _quizGroupService.CreateQuizGroupAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Lỗi database", result.Message);

        // Should rollback both files
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedImgKey, "quizgroups"), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedVideoKey, "quizgroups"), Times.Once);
    }

    #endregion

    #region UpdateQuizGroupAsync Tests

    [Fact]
    public async Task UpdateQuizGroupAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var quizGroupId = 1;
        var dto = new UpdateQuizGroupDto
        {
            Title = "Updated Quiz Group"
        };

        var existingQuizGroup = new QuizGroup
        {
            QuizGroupId = quizGroupId,
            Title = "Original Title",
            QuizSectionId = 1
        };

        var updatedQuizGroup = new QuizGroup
        {
            QuizGroupId = quizGroupId,
            Title = dto.Title,
            QuizSectionId = 1
        };

        var quizGroupDto = new QuizGroupDto
        {
            QuizGroupId = quizGroupId,
            Title = dto.Title
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(quizGroupId))
            .ReturnsAsync(existingQuizGroup);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateQuizGroupDto>(), It.IsAny<QuizGroup>()))
            .Returns(updatedQuizGroup);

        _quizGroupRepositoryMock
            .Setup(x => x.UpdateQuizGroupAsync(It.IsAny<QuizGroup>()))
            .ReturnsAsync(updatedQuizGroup);

        _mapperMock
            .Setup(x => x.Map<QuizGroupDto>(It.IsAny<QuizGroup>()))
            .Returns(quizGroupDto);

        // Act
        var result = await _quizGroupService.UpdateQuizGroupAsync(quizGroupId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Cập nhật nhóm câu hỏi thành công", result.Message);

        _quizGroupRepositoryMock.Verify(x => x.UpdateQuizGroupAsync(It.IsAny<QuizGroup>()), Times.Once);
    }

    [Fact]
    public async Task UpdateQuizGroupAsync_WithNonExistentQuizGroup_ReturnsNotFound()
    {
        // Arrange
        var quizGroupId = 999;
        var dto = new UpdateQuizGroupDto
        {
            Title = "Updated Quiz Group"
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(quizGroupId))
            .ReturnsAsync((QuizGroup?)null);

        // Act
        var result = await _quizGroupService.UpdateQuizGroupAsync(quizGroupId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy quiz group", result.Message);

        _quizGroupRepositoryMock.Verify(x => x.UpdateQuizGroupAsync(It.IsAny<QuizGroup>()), Times.Never);
    }

    #endregion

    #region DeleteQuizGroupAsync Tests

    [Fact]
    public async Task DeleteQuizGroupAsync_WithValidQuizGroup_ReturnsSuccess()
    {
        // Arrange
        var quizGroupId = 1;

        var quizGroup = new QuizGroup
        {
            QuizGroupId = quizGroupId,
            Title = "Test Quiz Group",
            ImgKey = "quizgroups/real/img-123",
            VideoKey = "quizgroups/real/video-123"
        };

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(quizGroupId))
            .ReturnsAsync(quizGroup);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(It.IsAny<string>(), "quizgroups"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        _quizGroupRepositoryMock
            .Setup(x => x.DeleteQuizGroupAsync(quizGroupId))
            .ReturnsAsync(true);

        // Act
        var result = await _quizGroupService.DeleteQuizGroupAsync(quizGroupId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Contains("Xóa nhóm câu hỏi thành công.", result.Message);

        // Should delete both image and video files
        // Note: DeleteFileAsync parameter order is (objectKey, bucketName)
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(quizGroup.ImgKey!, It.IsAny<string>()), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(quizGroup.VideoKey!, It.IsAny<string>()), Times.Once);
        _quizGroupRepositoryMock.Verify(x => x.DeleteQuizGroupAsync(quizGroupId), Times.Once);
    }

    [Fact]
    public async Task DeleteQuizGroupAsync_WithNonExistentQuizGroup_ReturnsNotFound()
    {
        // Arrange
        var quizGroupId = 999;

        _quizGroupRepositoryMock
            .Setup(x => x.GetQuizGroupByIdAsync(quizGroupId))
            .ReturnsAsync((QuizGroup?)null);

        // Act
        var result = await _quizGroupService.DeleteQuizGroupAsync(quizGroupId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy quiz group", result.Message);

        _quizGroupRepositoryMock.Verify(x => x.DeleteQuizGroupAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}

