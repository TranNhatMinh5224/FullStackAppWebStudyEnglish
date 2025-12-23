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

public class ModuleServiceTests
{
    private readonly Mock<IModuleRepository> _moduleRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ModuleService>> _loggerMock;
    private readonly Mock<ILessonRepository> _lessonRepositoryMock;
    private readonly Mock<ICourseRepository> _courseRepositoryMock;
    private readonly Mock<IModuleCompletionRepository> _moduleCompletionRepositoryMock;
    private readonly Mock<IMinioFileStorage> _minioFileStorageMock;
    private readonly ModuleService _moduleService;

    public ModuleServiceTests()
    {
        // Cấu hình BuildPublicUrl cho tests
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(x => x["Minio:BaseUrl"]).Returns("http://localhost:9000");
        BuildPublicUrl.Configure(configMock.Object);

        _moduleRepositoryMock = new Mock<IModuleRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ModuleService>>();
        _lessonRepositoryMock = new Mock<ILessonRepository>();
        _courseRepositoryMock = new Mock<ICourseRepository>();
        _moduleCompletionRepositoryMock = new Mock<IModuleCompletionRepository>();
        _minioFileStorageMock = new Mock<IMinioFileStorage>();

        _moduleService = new ModuleService(
            _moduleRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _loggerMock.Object,
            _lessonRepositoryMock.Object,
            _courseRepositoryMock.Object,
            _moduleCompletionRepositoryMock.Object,
            _minioFileStorageMock.Object
        );
    }

    #region GetModuleByIdAsync Tests

    [Fact]
    public async Task GetModuleByIdAsync_WithValidId_ReturnsModule()
    {
        // Arrange
        var moduleId = 1;
        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            Description = "Test Description",
            LessonId = 1,
            ContentType = ModuleType.FlashCard,
            ImageKey = "modules/real/image-123"
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = moduleId,
            Name = "Test Module",
            Description = "Test Description",
            LessonId = 1,
            ContentType = ModuleType.FlashCard
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(module);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(module))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.GetModuleByIdAsync(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(moduleId, result.Data.ModuleId);
        Assert.NotNull(result.Data.ImageUrl); // Should be built from ImageKey

        _moduleRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(moduleId), Times.Once);
    }

    [Fact]
    public async Task GetModuleByIdAsync_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var moduleId = 999;

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act
        var result = await _moduleService.GetModuleByIdAsync(moduleId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy module", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetModuleByIdAsync_WithNullImageKey_DoesNotBuildUrl()
    {
        // Arrange
        var moduleId = 1;
        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1,
            ImageKey = null
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(module);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(module))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.GetModuleByIdAsync(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Null(result.Data.ImageUrl);
    }

    [Fact]
    public async Task GetModuleByIdAsync_WithException_ReturnsError()
    {
        // Arrange
        var moduleId = 1;

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _moduleService.GetModuleByIdAsync(moduleId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("lỗi", result.Message);
    }

    #endregion

    #region GetModulesByLessonIdAsync Tests

    [Fact]
    public async Task GetModulesByLessonIdAsync_WithValidLessonId_ReturnsModules()
    {
        // Arrange
        var lessonId = 1;
        var modules = new List<Module>
        {
            new Module
            {
                ModuleId = 1,
                Name = "Module 1",
                LessonId = lessonId,
                OrderIndex = 1,
                ContentType = ModuleType.FlashCard
            },
            new Module
            {
                ModuleId = 2,
                Name = "Module 2",
                LessonId = lessonId,
                OrderIndex = 2,
                ContentType = ModuleType.Lecture
            }
        };

        var moduleDtos = new List<ListModuleDto>
        {
            new ListModuleDto
            {
                ModuleId = 1,
                Name = "Module 1",
                LessonId = lessonId,
                OrderIndex = 1
            },
            new ListModuleDto
            {
                ModuleId = 2,
                Name = "Module 2",
                LessonId = lessonId,
                OrderIndex = 2
            }
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdAsync(lessonId))
            .ReturnsAsync(modules);

        _mapperMock
            .Setup(x => x.Map<List<ListModuleDto>>(modules))
            .Returns(moduleDtos);

        // Act
        var result = await _moduleService.GetModulesByLessonIdAsync(lessonId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        _moduleRepositoryMock.Verify(x => x.GetByLessonIdAsync(lessonId), Times.Once);
    }

    [Fact]
    public async Task GetModulesByLessonIdAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var lessonId = 1;

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdAsync(lessonId))
            .ReturnsAsync(new List<Module>());

        _mapperMock
            .Setup(x => x.Map<List<ListModuleDto>>(It.IsAny<List<Module>>()))
            .Returns(new List<ListModuleDto>());

        // Act
        var result = await _moduleService.GetModulesByLessonIdAsync(lessonId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region CreateModuleAsync Tests

    [Fact]
    public async Task CreateModuleAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateModuleDto
        {
            LessonId = 1,
            Name = "Test Module",
            Description = "Test Description",
            OrderIndex = 1,
            ContentType = ModuleType.FlashCard
        };

        var module = new Module
        {
            ModuleId = 1,
            Name = dto.Name,
            Description = dto.Description,
            LessonId = dto.LessonId,
            OrderIndex = dto.OrderIndex,
            ContentType = dto.ContentType
        };

        var moduleWithDetails = new Module
        {
            ModuleId = 1,
            Name = dto.Name,
            Description = dto.Description,
            LessonId = dto.LessonId,
            OrderIndex = dto.OrderIndex,
            ContentType = dto.ContentType
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = 1,
            Name = dto.Name,
            Description = dto.Description,
            LessonId = dto.LessonId,
            OrderIndex = dto.OrderIndex,
            ContentType = dto.ContentType
        };

        _moduleRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.LessonId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Module>(dto))
            .Returns(module);

        _moduleRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Module>()))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(module.ModuleId))
            .ReturnsAsync(moduleWithDetails);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(moduleWithDetails))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.CreateModuleAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode); // Create operations return 201
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Name, result.Data.Name);

        _moduleRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Module>()), Times.Once);
    }

    [Fact]
    public async Task CreateModuleAsync_AsTeacherWithOwnCourse_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateModuleDto
        {
            LessonId = 1,
            Name = "Test Module",
            OrderIndex = 1,
            ContentType = ModuleType.FlashCard
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.Teacher,
            TeacherId = userId
        };

        var module = new Module
        {
            ModuleId = 1,
            Name = dto.Name,
            LessonId = dto.LessonId,
            OrderIndex = dto.OrderIndex,
            ContentType = dto.ContentType
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = 1,
            Name = dto.Name,
            LessonId = dto.LessonId
        };

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(dto.LessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        _moduleRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.LessonId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Module>(dto))
            .Returns(module);

        _moduleRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Module>()))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(module.ModuleId))
            .ReturnsAsync(module);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(It.IsAny<Module>()))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.CreateModuleAsync(dto, userId, "Teacher");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);

        _lessonRepositoryMock.Verify(x => x.GetLessonById(dto.LessonId), Times.Once);
        _courseRepositoryMock.Verify(x => x.GetCourseById(lesson.CourseId), Times.Once);
    }

    [Fact]
    public async Task CreateModuleAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var userId = 1;
        var ownerId = 2;
        var dto = new CreateModuleDto
        {
            LessonId = 1,
            Name = "Test Module",
            OrderIndex = 1,
            ContentType = ModuleType.FlashCard
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(dto.LessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _moduleService.CreateModuleAsync(dto, userId, "Teacher");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Bạn không có quyền tạo module", result.Message);

        _moduleRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Module>()), Times.Never);
    }

    [Fact]
    public async Task CreateModuleAsync_WithImageTempKey_CommitsImageAndSavesKey()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateModuleDto
        {
            LessonId = 1,
            Name = "Test Module",
            OrderIndex = 1,
            ContentType = ModuleType.FlashCard,
            ImageTempKey = "temp/image-123",
            ImageType = "image/jpeg"
        };

        var module = new Module
        {
            ModuleId = 1,
            Name = dto.Name,
            LessonId = dto.LessonId,
            ImageKey = "modules/real/image-123",
            ImageType = dto.ImageType
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = 1,
            Name = dto.Name,
            ImageType = dto.ImageType
        };

        _moduleRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.LessonId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Module>(dto))
            .Returns(module);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "modules", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "modules/real/image-123"
            });

        _moduleRepositoryMock
            .Setup(x => x.CreateAsync(It.Is<Module>(m => m.ImageKey == "modules/real/image-123")))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(module.ModuleId))
            .ReturnsAsync(module);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(It.IsAny<Module>()))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.CreateModuleAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.NotNull(result.Data.ImageUrl); // Should be built from ImageKey

        _minioFileStorageMock.Verify(x => x.CommitFileAsync(dto.ImageTempKey, "modules", "real"), Times.Once);
    }

    [Fact]
    public async Task CreateModuleAsync_WithImageCommitFailure_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateModuleDto
        {
            LessonId = 1,
            Name = "Test Module",
            OrderIndex = 1,
            ContentType = ModuleType.FlashCard,
            ImageTempKey = "temp/image-123"
        };

        var module = new Module
        {
            ModuleId = 1,
            Name = dto.Name,
            LessonId = dto.LessonId
        };

        _moduleRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.LessonId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Module>(dto))
            .Returns(module);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "modules", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = false,
                Message = "Failed to commit file"
            });

        // Act
        var result = await _moduleService.CreateModuleAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Không thể lưu ảnh", result.Message);

        _moduleRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Module>()), Times.Never);
    }

    [Fact]
    public async Task CreateModuleAsync_WithDatabaseError_RollsBackImage()
    {
        // Arrange
        var userId = 1;
        var dto = new CreateModuleDto
        {
            LessonId = 1,
            Name = "Test Module",
            OrderIndex = 1,
            ContentType = ModuleType.FlashCard,
            ImageTempKey = "temp/image-123",
            ImageType = "image/jpeg"
        };

        var module = new Module
        {
            ModuleId = 1,
            Name = dto.Name,
            LessonId = dto.LessonId
        };

        _moduleRepositoryMock
            .Setup(x => x.GetMaxOrderIndexAsync(dto.LessonId))
            .ReturnsAsync(0);

        _mapperMock
            .Setup(x => x.Map<Module>(dto))
            .Returns(module);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "modules", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "modules/real/image-123"
            });

        _moduleRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Module>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _moduleService.CreateModuleAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        // Verify rollback
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("modules/real/image-123", "modules"), Times.Once);
    }

    #endregion

    #region UpdateModuleAsync Tests

    [Fact]
    public async Task UpdateModuleAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var dto = new UpdateModuleDto
        {
            Name = "Updated Module",
            Description = "Updated Description"
        };

        var existingModule = new Module
        {
            ModuleId = moduleId,
            Name = "Original Module",
            Description = "Original Description",
            LessonId = 1
        };

        var updatedModule = new Module
        {
            ModuleId = moduleId,
            Name = dto.Name,
            Description = dto.Description,
            LessonId = 1
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = moduleId,
            Name = dto.Name,
            Description = dto.Description,
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(existingModule);

        _moduleRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Module>()))
            .ReturnsAsync(updatedModule);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(updatedModule);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateModuleDto>(), It.IsAny<Module>()))
            .Returns(updatedModule);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(updatedModule))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.UpdateModuleAsync(moduleId, dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Name, result.Data.Name);

        _moduleRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Module>()), Times.Once);
    }

    [Fact]
    public async Task UpdateModuleAsync_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var moduleId = 999;
        var userId = 1;
        var dto = new UpdateModuleDto { Name = "Updated Module" };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act
        var result = await _moduleService.UpdateModuleAsync(moduleId, dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy module", result.Message);

        _moduleRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Module>()), Times.Never);
    }

    [Fact]
    public async Task UpdateModuleAsync_WithNewImage_DeletesOldImage()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var dto = new UpdateModuleDto
        {
            Name = "Updated Module",
            ImageTempKey = "temp/new-image-123",
            ImageType = "image/jpeg"
        };

        var existingModule = new Module
        {
            ModuleId = moduleId,
            Name = "Original Module",
            LessonId = 1,
            ImageKey = "modules/real/old-image-123",
            ImageType = "image/png"
        };

        var updatedModule = new Module
        {
            ModuleId = moduleId,
            Name = dto.Name,
            LessonId = 1,
            ImageKey = "modules/real/new-image-123",
            ImageType = dto.ImageType
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = moduleId,
            Name = dto.Name
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(existingModule);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "modules", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "modules/real/new-image-123"
            });

        _moduleRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Module>()))
            .ReturnsAsync(updatedModule);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(updatedModule);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateModuleDto>(), It.IsAny<Module>()))
            .Returns(updatedModule);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(updatedModule))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.UpdateModuleAsync(moduleId, dto, userId);

        // Assert
        Assert.True(result.Success);

        // Verify old image is deleted
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("modules/real/old-image-123", "modules"), Times.Once);
    }

    [Fact]
    public async Task UpdateModuleAsync_WithDatabaseError_RollsBackNewImage()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var dto = new UpdateModuleDto
        {
            Name = "Updated Module",
            ImageTempKey = "temp/new-image-123"
        };

        var existingModule = new Module
        {
            ModuleId = moduleId,
            Name = "Original Module",
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(existingModule);

        _minioFileStorageMock
            .Setup(x => x.CommitFileAsync(dto.ImageTempKey, "modules", "real"))
            .ReturnsAsync(new ServiceResponse<string>
            {
                Success = true,
                Data = "modules/real/new-image-123"
            });

        _moduleRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Module>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _moduleService.UpdateModuleAsync(moduleId, dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);

        // Verify rollback
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("modules/real/new-image-123", "modules"), Times.Once);
    }

    #endregion

    #region DeleteModuleAsync Tests

    [Fact]
    public async Task DeleteModuleAsync_WithValidModule_ReturnsSuccess()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1,
            ImageKey = "modules/real/image-123"
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.DeleteAsync(moduleId))
            .ReturnsAsync(true);

        // Act
        var result = await _moduleService.DeleteModuleAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Contains("Xóa module thành công", result.Message);

        _moduleRepositoryMock.Verify(x => x.DeleteAsync(moduleId), Times.Once);
        _minioFileStorageMock.Verify(x => x.DeleteFileAsync("modules/real/image-123", "modules"), Times.Once);
    }

    [Fact]
    public async Task DeleteModuleAsync_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var moduleId = 999;
        var userId = 1;

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act
        var result = await _moduleService.DeleteModuleAsync(moduleId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy module", result.Message);

        _moduleRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteModuleAsync_WithNoImage_DoesNotCallMinio()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1,
            ImageKey = null
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.DeleteAsync(moduleId))
            .ReturnsAsync(true);

        // Act
        var result = await _moduleService.DeleteModuleAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);

        _minioFileStorageMock.Verify(x => x.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region GetModulesWithProgressAsync Tests

    [Fact]
    public async Task GetModulesWithProgressAsync_WithValidLessonId_ReturnsModulesWithProgress()
    {
        // Arrange
        var lessonId = 1;
        var userId = 1;

        var modules = new List<Module>
        {
            new Module
            {
                ModuleId = 1,
                Name = "Module 1",
                LessonId = lessonId,
                OrderIndex = 1
            },
            new Module
            {
                ModuleId = 2,
                Name = "Module 2",
                LessonId = lessonId,
                OrderIndex = 2
            }
        };

        var moduleCompletion = new ModuleCompletion
        {
            ModuleCompletionId = 1,
            UserId = userId,
            ModuleId = 1,
            IsCompleted = true,
            ProgressPercentage = 100
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdWithDetailsAsync(lessonId))
            .ReturnsAsync(modules);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleIdsAsync(userId, It.IsAny<List<int>>()))
            .ReturnsAsync(new List<ModuleCompletion> { moduleCompletion });

        _mapperMock
            .Setup(x => x.Map<ModuleWithProgressDto>(It.IsAny<Module>()))
            .Returns<Module>(m => new ModuleWithProgressDto
            {
                ModuleId = m.ModuleId,
                Name = m.Name,
                LessonId = m.LessonId,
                OrderIndex = m.OrderIndex
            });

        // Act
        var result = await _moduleService.GetModulesWithProgressAsync(lessonId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        var module1 = result.Data.First(m => m.ModuleId == 1);
        Assert.True(module1.IsCompleted);
        Assert.Equal(100, module1.ProgressPercentage);

        var module2 = result.Data.First(m => m.ModuleId == 2);
        Assert.False(module2.IsCompleted);
        Assert.Equal(0, module2.ProgressPercentage);

        _moduleRepositoryMock.Verify(x => x.GetByLessonIdWithDetailsAsync(lessonId), Times.Once);
    }

    [Fact]
    public async Task GetModulesWithProgressAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var lessonId = 1;
        var userId = 1;

        _moduleRepositoryMock
            .Setup(x => x.GetByLessonIdWithDetailsAsync(lessonId))
            .ReturnsAsync(new List<Module>());

        // Act
        var result = await _moduleService.GetModulesWithProgressAsync(lessonId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region GetModuleWithProgressAsync Tests

    [Fact]
    public async Task GetModuleWithProgressAsync_WithValidModule_ReturnsModuleWithProgress()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1,
            OrderIndex = 1
        };

        var moduleCompletion = new ModuleCompletion
        {
            ModuleCompletionId = 1,
            UserId = userId,
            ModuleId = moduleId,
            IsCompleted = true,
            ProgressPercentage = 100,
            StartedAt = DateTime.UtcNow.AddDays(-5),
            CompletedAt = DateTime.UtcNow
        };

        var moduleDto = new ModuleWithProgressDto
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync(moduleCompletion);

        _mapperMock
            .Setup(x => x.Map<ModuleWithProgressDto>(module))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.GetModuleWithProgressAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(moduleId, result.Data.ModuleId);
        Assert.True(result.Data.IsCompleted);
        Assert.Equal(100, result.Data.ProgressPercentage);
        Assert.NotNull(result.Data.StartedAt);
        Assert.NotNull(result.Data.CompletedAt);

        _moduleRepositoryMock.Verify(x => x.GetByIdWithDetailsAsync(moduleId), Times.Once);
        _moduleCompletionRepositoryMock.Verify(x => x.GetByUserAndModuleAsync(userId, moduleId), Times.Once);
    }

    [Fact]
    public async Task GetModuleWithProgressAsync_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var moduleId = 999;
        var userId = 1;

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync((Module?)null);

        // Act
        var result = await _moduleService.GetModuleWithProgressAsync(moduleId, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy module", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task GetModuleWithProgressAsync_WithNoProgress_ReturnsDefaultValues()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1
        };

        var moduleDto = new ModuleWithProgressDto
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(module);

        _moduleCompletionRepositoryMock
            .Setup(x => x.GetByUserAndModuleAsync(userId, moduleId))
            .ReturnsAsync((ModuleCompletion?)null);

        _mapperMock
            .Setup(x => x.Map<ModuleWithProgressDto>(module))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.GetModuleWithProgressAsync(moduleId, userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.False(result.Data.IsCompleted);
        Assert.Equal(0, result.Data.ProgressPercentage);
        Assert.Null(result.Data.StartedAt);
        Assert.Null(result.Data.CompletedAt);
    }

    #endregion

    #region UpdateModuleWithAuthorizationAsync Tests

    [Fact]
    public async Task UpdateModuleWithAuthorizationAsync_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var dto = new UpdateModuleDto { Name = "Updated Module" };

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Original Module",
            LessonId = 1
        };

        var updatedModule = new Module
        {
            ModuleId = moduleId,
            Name = dto.Name,
            LessonId = 1
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = moduleId,
            Name = dto.Name,
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Module>()))
            .ReturnsAsync(updatedModule);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(updatedModule);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateModuleDto>(), It.IsAny<Module>()))
            .Returns(updatedModule);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(updatedModule))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.UpdateModuleWithAuthorizationAsync(moduleId, dto, userId, "Admin");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Name, result.Data.Name);

        _courseRepositoryMock.Verify(x => x.GetCourseById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateModuleWithAuthorizationAsync_AsTeacherWithOwnCourse_ReturnsSuccess()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var dto = new UpdateModuleDto { Name = "Updated Module" };

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Original Module",
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.Teacher,
            TeacherId = userId
        };

        var updatedModule = new Module
        {
            ModuleId = moduleId,
            Name = dto.Name,
            LessonId = 1
        };

        var moduleDto = new ModuleDto
        {
            ModuleId = moduleId,
            Name = dto.Name,
            LessonId = 1
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(module.LessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        _moduleRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Module>()))
            .ReturnsAsync(updatedModule);

        _moduleRepositoryMock
            .Setup(x => x.GetByIdWithDetailsAsync(moduleId))
            .ReturnsAsync(updatedModule);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateModuleDto>(), It.IsAny<Module>()))
            .Returns(updatedModule);

        _mapperMock
            .Setup(x => x.Map<ModuleDto>(updatedModule))
            .Returns(moduleDto);

        // Act
        var result = await _moduleService.UpdateModuleWithAuthorizationAsync(moduleId, dto, userId, "Teacher");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);

        _courseRepositoryMock.Verify(x => x.GetCourseById(lesson.CourseId), Times.Once);
    }

    [Fact]
    public async Task UpdateModuleWithAuthorizationAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var ownerId = 2;
        var dto = new UpdateModuleDto { Name = "Updated Module" };

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Original Module",
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(module.LessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _moduleService.UpdateModuleWithAuthorizationAsync(moduleId, dto, userId, "Teacher");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Bạn không có quyền chỉnh sửa module", result.Message);

        _moduleRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Module>()), Times.Never);
    }

    #endregion

    #region DeleteModuleWithAuthorizationAsync Tests

    [Fact]
    public async Task DeleteModuleWithAuthorizationAsync_AsAdmin_ReturnsSuccess()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1,
            ImageKey = "modules/real/image-123"
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _moduleRepositoryMock
            .Setup(x => x.DeleteAsync(moduleId))
            .ReturnsAsync(true);

        // Act
        var result = await _moduleService.DeleteModuleWithAuthorizationAsync(moduleId, userId, "Admin");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);
        Assert.Contains("Xóa module thành công", result.Message);

        _courseRepositoryMock.Verify(x => x.GetCourseById(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteModuleWithAuthorizationAsync_AsTeacherWithOwnCourse_ReturnsSuccess()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.Teacher,
            TeacherId = userId
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(module.LessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        _moduleRepositoryMock
            .Setup(x => x.DeleteAsync(moduleId))
            .ReturnsAsync(true);

        // Act
        var result = await _moduleService.DeleteModuleWithAuthorizationAsync(moduleId, userId, "Teacher");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);

        _courseRepositoryMock.Verify(x => x.GetCourseById(lesson.CourseId), Times.Once);
    }

    [Fact]
    public async Task DeleteModuleWithAuthorizationAsync_AsTeacherWithWrongCourse_ReturnsForbidden()
    {
        // Arrange
        var moduleId = 1;
        var userId = 1;
        var ownerId = 2;

        var module = new Module
        {
            ModuleId = moduleId,
            Name = "Test Module",
            LessonId = 1
        };

        var lesson = new Lesson
        {
            LessonId = 1,
            Title = "Test Lesson",
            CourseId = 1
        };

        var course = new Course
        {
            CourseId = 1,
            Title = "Test Course",
            Type = CourseType.Teacher,
            TeacherId = ownerId // Different owner
        };

        _moduleRepositoryMock
            .Setup(x => x.GetByIdAsync(moduleId))
            .ReturnsAsync(module);

        _lessonRepositoryMock
            .Setup(x => x.GetLessonById(module.LessonId))
            .ReturnsAsync(lesson);

        _courseRepositoryMock
            .Setup(x => x.GetCourseById(lesson.CourseId))
            .ReturnsAsync(course);

        // Act
        var result = await _moduleService.DeleteModuleWithAuthorizationAsync(moduleId, userId, "Teacher");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Bạn không có quyền xóa module", result.Message);
        Assert.False(result.Data);

        _moduleRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>()), Times.Never);
    }

    #endregion
}

