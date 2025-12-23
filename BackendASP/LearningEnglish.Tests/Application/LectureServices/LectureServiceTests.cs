using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Configuration;

namespace LearningEnglish.Tests.Application;

public class LectureServiceTests
{
    private readonly Mock<ILectureRepository> _lectureRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<LectureService>> _loggerMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly LectureService _lectureService;

    public LectureServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _lectureRepositoryMock = new Mock<ILectureRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<LectureService>>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _lectureService = new LectureService(
            _lectureRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetLectureByIdAsync Tests

    [Fact]
    public async Task GetLectureByIdAsync_WithValidId_ReturnsLecture()
    {
        // Arrange
        var lectureId = 1;
        var lecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1,
            Type = LectureType.Content,
            MediaKey = "lectures/real/media-123"
        };

        var lectureDto = new LectureDto
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1,
            Type = LectureType.Content,
            MediaUrl = "lectures/real/media-123"
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(lectureId))
            .ReturnsAsync(lecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(lecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.GetLectureByIdAsync(lectureId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(lectureId, result.Data.LectureId);
        Assert.NotNull(result.Data.MediaUrl); // Should be built from MediaKey

        _lectureRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(lectureId), Times.Once);
    }

    [Fact]
    public async Task GetLectureByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var lectureId = 999;

        _lectureRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(lectureId))
            .ReturnsAsync((Lecture?)null);

        // Act
        var result = await _lectureService.GetLectureByIdAsync(lectureId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy lecture", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetLectureByIdAsync_WithNullMedia_DoesNotBuildUrl()
    {
        // Arrange
        var lectureId = 1;
        var lecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1,
            Type = LectureType.Content,
            MediaKey = null
        };

        var lectureDto = new LectureDto
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1,
            Type = LectureType.Content
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(lectureId))
            .ReturnsAsync(lecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(lecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.GetLectureByIdAsync(lectureId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.MediaUrl);
    }

    #endregion

    #region GetLecturesByModuleIdAsync Tests

    [Fact]
    public async Task GetLecturesByModuleIdAsync_WithValidModuleId_ReturnsLectures()
    {
        // Arrange
        var moduleId = 1;
        var lectures = new List<Lecture>
        {
            new Lecture
            {
                LectureId = 1,
                Title = "Lecture 1",
                ModuleId = moduleId,
                Type = LectureType.Content
            },
            new Lecture
            {
                LectureId = 2,
                Title = "Lecture 2",
                ModuleId = moduleId,
                Type = LectureType.Video
            }
        };

        var lectureDtos = new List<ListLectureDto>
        {
            new ListLectureDto
            {
                LectureId = 1,
                Title = "Lecture 1",
                ModuleId = moduleId
            },
            new ListLectureDto
            {
                LectureId = 2,
                Title = "Lecture 2",
                ModuleId = moduleId
            }
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByModuleIdWithDetailsAsync(moduleId))
            .ReturnsAsync(lectures);

        _mapperMock
            .Setup(x => x.Map<List<ListLectureDto>>(lectures))
            .Returns(lectureDtos);

        // Act
        var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains("lecture thành công", result.Message);

        _lectureRepositoryMock.Verify(x => x.GetByModuleIdWithDetailsAsync(moduleId), Times.Once);
    }

    [Fact]
    public async Task GetLecturesByModuleIdAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var moduleId = 1;

        _lectureRepositoryMock
            .Setup(x => x.GetByModuleIdWithDetailsAsync(moduleId))
            .ReturnsAsync(new List<Lecture>());

        _mapperMock
            .Setup(x => x.Map<List<ListLectureDto>>(It.IsAny<List<Lecture>>()))
            .Returns(new List<ListLectureDto>());

        // Act
        var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region CreateLectureAsync Tests

    [Fact]
    public async Task CreateLectureAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateLectureDto
        {
            ModuleId = 1,
            Title = "New Lecture",
            Type = LectureType.Content,
            OrderIndex = 1
        };

        var lecture = new Lecture
        {
            LectureId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId,
            Type = dto.Type,
            OrderIndex = dto.OrderIndex
        };

        var lectureDto = new LectureDto
        {
            LectureId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId,
            Type = dto.Type
        };

        _lectureRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.ModuleId, null))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Lecture>(dto))
            .Returns(lecture);

        _lectureRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Lecture>()))
            .ReturnsAsync(lecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(lecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.CreateLectureAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Tạo lecture thành công", result.Message);

        _lectureRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Lecture>()), Times.Once);
    }

    [Fact]
    public async Task CreateLectureAsync_WithMedia_CommitsFile()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateLectureDto
        {
            ModuleId = 1,
            Title = "New Lecture",
            Type = LectureType.Video,
            OrderIndex = 1,
            MediaTempKey = "temp/media-123"
        };

        var lecture = new Lecture
        {
            LectureId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId,
            Type = dto.Type,
            MediaKey = "lectures/real/media-123"
        };

        var lectureDto = new LectureDto
        {
            LectureId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId,
            Type = dto.Type
        };

        _lectureRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.ModuleId, null))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Lecture>(dto))
            .Returns(lecture);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.MediaTempKey, "lectures", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "lectures/real/media-123"
            });

        _lectureRepositoryMock
            .Setup(x => x.CreateAsync(It.Is<Lecture>(l => 
                l.MediaKey == "lectures/real/media-123")))
            .ReturnsAsync(lecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(lecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.CreateLectureAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.MediaTempKey, "lectures", "real"), Times.Once);
    }

    [Fact]
    public async Task CreateLectureAsync_WithInvalidParent_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateLectureDto
        {
            ModuleId = 1,
            Title = "New Lecture",
            Type = LectureType.Content,
            OrderIndex = 1,
            ParentLectureId = 999
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.ParentLectureId.Value))
            .ReturnsAsync((Lecture?)null);

        // Act
        var result = await _lectureService.CreateLectureAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Parent lecture không tồn tại", result.Message);

        _lectureRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Lecture>()), Times.Never);
    }

    [Fact]
    public async Task CreateLectureAsync_WithParentFromDifferentModule_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateLectureDto
        {
            ModuleId = 1,
            Title = "New Lecture",
            Type = LectureType.Content,
            OrderIndex = 1,
            ParentLectureId = 2
        };

        var parentLecture = new Lecture
        {
            LectureId = 2,
            ModuleId = 2 // Different module
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(dto.ParentLectureId.Value))
            .ReturnsAsync(parentLecture);

        // Act
        var result = await _lectureService.CreateLectureAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Parent lecture phải thuộc cùng module", result.Message);

        _lectureRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Lecture>()), Times.Never);
    }

    [Fact]
    public async Task CreateLectureAsync_WithMediaCommitFailure_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateLectureDto
        {
            ModuleId = 1,
            Title = "New Lecture",
            Type = LectureType.Video,
            OrderIndex = 1,
            MediaTempKey = "temp/media-123"
        };

        var lecture = new Lecture
        {
            LectureId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId,
            Type = dto.Type
        };

        _lectureRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.ModuleId, null))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Lecture>(dto))
            .Returns(lecture);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.MediaTempKey, "lectures", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit file"
            });

        // Act
        var result = await _lectureService.CreateLectureAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không thể lưu media", result.Message);

        _lectureRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Lecture>()), Times.Never);
    }

    [Fact]
    public async Task CreateLectureAsync_WithDatabaseError_RollsBackFile()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateLectureDto
        {
            ModuleId = 1,
            Title = "New Lecture",
            Type = LectureType.Video,
            OrderIndex = 1,
            MediaTempKey = "temp/media-123"
        };

        var lecture = new Lecture
        {
            LectureId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId,
            Type = dto.Type
        };

        var committedKey = "lectures/real/media-123";

        _lectureRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.ModuleId, null))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Lecture>(dto))
            .Returns(lecture);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.MediaTempKey, "lectures", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = committedKey
            });

        _lectureRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Lecture>()))
            .ThrowsAsync(new Exception("Database error"));

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(committedKey, "lectures"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _lectureService.CreateLectureAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Lỗi database", result.Message);

        // Should rollback by deleting the committed file
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(committedKey, "lectures"), Times.Once);
    }

    #endregion

    #region UpdateLectureAsync Tests

    [Fact]
    public async Task UpdateLectureAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;
        var dto = new UpdateLectureDto
        {
            Title = "Updated Lecture"
        };

        var existingLecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Original Lecture",
            ModuleId = 1
        };

        var updatedLecture = new Lecture
        {
            LectureId = lectureId,
            Title = dto.Title,
            ModuleId = 1
        };

        var lectureDto = new LectureDto
        {
            LectureId = lectureId,
            Title = dto.Title,
            ModuleId = 1
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(existingLecture);

        _lectureRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Lecture>()))
            .ReturnsAsync(updatedLecture);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateLectureDto>(), It.IsAny<Lecture>()))
            .Returns(updatedLecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(updatedLecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.UpdateLectureAsync(lectureId, dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);

        _lectureRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lecture>()), Times.Once);
    }

    [Fact]
    public async Task UpdateLectureAsync_WithNonExistentLecture_ReturnsNotFound()
    {
        // Arrange
        var lectureId = 999;
        var userId = 1;
        var dto = new UpdateLectureDto
        {
            Title = "Updated Lecture"
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync((Lecture?)null);

        // Act
        var result = await _lectureService.UpdateLectureAsync(lectureId, dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy lecture", result.Message);

        _lectureRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lecture>()), Times.Never);
    }

    [Fact]
    public async Task UpdateLectureAsync_WithSelfAsParent_ReturnsError()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;
        var dto = new UpdateLectureDto
        {
            Title = "Updated Lecture",
            ParentLectureId = lectureId // Self reference
        };

        var existingLecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Original Lecture",
            ModuleId = 1
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(existingLecture);

        // Act
        var result = await _lectureService.UpdateLectureAsync(lectureId, dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Lecture không thể là parent của chính mình", result.Message);

        _lectureRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lecture>()), Times.Never);
    }

    #endregion

    #region DeleteLectureAsync Tests

    [Fact]
    public async Task DeleteLectureAsync_WithValidLecture_ReturnsSuccess()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;

        var lecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1,
            MediaKey = "lectures/real/media-123"
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(lecture);

        _lectureRepositoryMock
            .Setup(x => x.DeleteAsync(lectureId))
            .ReturnsAsync(true);

        _minioFileStorageMock
            .Setup(x => x.DeleteFileAsync(lecture.MediaKey, "lectures"))
            .ReturnsAsync(new ServiceResponse<bool> { Success = true, Data = true });

        // Act
        var result = await _lectureService.DeleteLectureAsync(lectureId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Contains("Xóa lecture thành công", result.Message);

        _lectureRepositoryMock.Verify(x => x.DeleteAsync(lectureId), Times.Once);
    }

    [Fact]
    public async Task DeleteLectureAsync_WithNonExistentLecture_ReturnsNotFound()
    {
        // Arrange
        var lectureId = 999;
        var userId = 1;

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync((Lecture?)null);

        // Act
        var result = await _lectureService.DeleteLectureAsync(lectureId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không tìm thấy lecture", result.Message);

        _lectureRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteLectureAsync_WithChildren_ReturnsError()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;

        var lecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(lecture);

        _lectureRepositoryMock
            .Setup(x => x.HasChildrenAsync(lectureId))
            .ReturnsAsync(true);

        // Act
        var result = await _lectureService.DeleteLectureAsync(lectureId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Không thể xóa lecture có lecture con", result.Message);

        _lectureRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region UpdateLectureWithAuthorizationAsync Tests

    [Fact]
    public async Task UpdateLectureWithAuthorizationAsync_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;
        var userRole = "Admin";
        var dto = new UpdateLectureDto
        {
            Title = "Updated Lecture"
        };

        var existingLecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Original Lecture",
            ModuleId = 1
        };

        var updatedLecture = new Lecture
        {
            LectureId = lectureId,
            Title = dto.Title,
            ModuleId = 1
        };

        var lectureDto = new LectureDto
        {
            LectureId = lectureId,
            Title = dto.Title
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(existingLecture);

        _lectureRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Lecture>()))
            .ReturnsAsync(updatedLecture);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateLectureDto>(), It.IsAny<Lecture>()))
            .Returns(updatedLecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(updatedLecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.UpdateLectureWithAuthorizationAsync(lectureId, dto, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateLectureWithAuthorizationAsync_AsTeacherWithOwnCourse_ReturnsSuccess()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;
        var userRole = "Teacher";
        var dto = new UpdateLectureDto
        {
            Title = "Updated Lecture"
        };

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

        var existingLecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Original Lecture",
            ModuleId = 1,
            Module = module
        };

        existingLecture.Module!.Lesson = lesson;
        lesson.Course = course;

        var updatedLecture = new Lecture
        {
            LectureId = lectureId,
            Title = dto.Title,
            ModuleId = 1
        };

        var lectureDto = new LectureDto
        {
            LectureId = lectureId,
            Title = dto.Title
        };

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync(existingLecture);

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(existingLecture);

        _lectureRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Lecture>()))
            .ReturnsAsync(updatedLecture);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateLectureDto>(), It.IsAny<Lecture>()))
            .Returns(updatedLecture);

        _mapperMock
            .Setup(x => x.Map<LectureDto>(updatedLecture))
            .Returns(lectureDto);

        // Act
        var result = await _lectureService.UpdateLectureWithAuthorizationAsync(lectureId, dto, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task UpdateLectureWithAuthorizationAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;
        var ownerId = 2;
        var userRole = "Teacher";
        var dto = new UpdateLectureDto
        {
            Title = "Updated Lecture"
        };

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

        var existingLecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Original Lecture",
            ModuleId = 1,
            Module = module
        };

        existingLecture.Module!.Lesson = lesson;
        lesson.Course = course;

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync(existingLecture);

        // Act
        var result = await _lectureService.UpdateLectureWithAuthorizationAsync(lectureId, dto, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Bạn không có quyền cập nhật lecture này", result.Message);

        _lectureRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Lecture>()), Times.Never);
    }

    #endregion

    #region DeleteLectureWithAuthorizationAsync Tests

    [Fact]
    public async Task DeleteLectureWithAuthorizationAsync_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var lectureId = 1;
        var userId = 1;
        var userRole = "Admin";

        var lecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1
        };

        _lectureRepositoryMock
            .Setup(x => x.GetByIdAsync(lectureId))
            .ReturnsAsync(lecture);

        _lectureRepositoryMock
            .Setup(x => x.DeleteAsync(lectureId))
            .ReturnsAsync(true);

        // Act
        var result = await _lectureService.DeleteLectureWithAuthorizationAsync(lectureId, userId, userRole);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteLectureWithAuthorizationAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var lectureId = 1;
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

        var lecture = new Lecture
        {
            LectureId = lectureId,
            Title = "Test Lecture",
            ModuleId = 1,
            Module = module
        };

        lecture.Module!.Lesson = lesson;
        lesson.Course = course;

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync(lecture);

        // Act
        var result = await _lectureService.DeleteLectureWithAuthorizationAsync(lectureId, userId, userRole);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Bạn không có quyền xóa lecture này", result.Message);

        _lectureRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion

    #region CheckTeacherLecturePermission Tests

    [Fact]
    public async Task CheckTeacherLecturePermission_WithOwnCourse_ReturnsTrue()
    {
        // Arrange
        var lectureId = 1;
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

        var lecture = new Lecture
        {
            LectureId = lectureId,
            ModuleId = 1,
            Module = module
        };

        lecture.Module!.Lesson = lesson;
        lesson.Course = course;

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync(lecture);

        // Act
        var result = await _lectureService.CheckTeacherLecturePermission(lectureId, teacherId);

        // Assert
        Assert.True(result);

        _lectureRepositoryMock.Verify(x => x.GetLectureWithModuleCourseAsync(lectureId), Times.Once);
    }

    [Fact]
    public async Task CheckTeacherLecturePermission_WithWrongCourse_ReturnsFalse()
    {
        // Arrange
        var lectureId = 1;
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

        var lecture = new Lecture
        {
            LectureId = lectureId,
            ModuleId = 1,
            Module = module
        };

        lecture.Module!.Lesson = lesson;
        lesson.Course = course;

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync(lecture);

        // Act
        var result = await _lectureService.CheckTeacherLecturePermission(lectureId, teacherId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckTeacherLecturePermission_WithSystemCourse_ReturnsFalse()
    {
        // Arrange
        var lectureId = 1;
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

        var lecture = new Lecture
        {
            LectureId = lectureId,
            ModuleId = 1,
            Module = module
        };

        lecture.Module!.Lesson = lesson;
        lesson.Course = course;

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync(lecture);

        // Act
        var result = await _lectureService.CheckTeacherLecturePermission(lectureId, teacherId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task CheckTeacherLecturePermission_WithNonExistentLecture_ReturnsFalse()
    {
        // Arrange
        var lectureId = 999;
        var teacherId = 1;

        _lectureRepositoryMock
            .Setup(x => x.GetLectureWithModuleCourseAsync(lectureId))
            .ReturnsAsync((Lecture?)null);

        // Act
        var result = await _lectureService.CheckTeacherLecturePermission(lectureId, teacherId);

        // Assert
        Assert.False(result);
    }

    #endregion
}

