using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using Moq;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace LearningEnglish.Tests.Application.AssessmentServices;

public class AssessmentServiceTests
{
    private readonly Mock<IAssessmentRepository> _assessmentRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<AssessmentService>> _loggerMock;
    private readonly AssessmentService _assessmentService;

    public AssessmentServiceTests()
    {
        _assessmentRepositoryMock = new Mock<IAssessmentRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<AssessmentService>>();

        _assessmentService = new AssessmentService(
            _assessmentRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    #region GetAssessmentById Tests

    [Fact]
    public async Task GetAssessmentById_WithValidId_ReturnsAssessment()
    {
        // Arrange
        var assessmentId = 1;
        var assessment = new Assessment
        {
            AssessmentId = assessmentId,
            Title = "Test Assessment",
            ModuleId = 1
        };

        var assessmentDto = new AssessmentDto
        {
            AssessmentId = assessmentId,
            Title = "Test Assessment",
            ModuleId = 1
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync(assessment);

        _mapperMock
            .Setup(x => x.Map<AssessmentDto>(assessment))
            .Returns(assessmentDto);

        // Act
        var result = await _assessmentService.GetAssessmentById(assessmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(assessmentId, result.Data.AssessmentId);
        Assert.Equal("Test Assessment", result.Data.Title);

        _assessmentRepositoryMock.Verify(x => x.GetAssessmentById(assessmentId), Times.Once);
    }

    [Fact]
    public async Task GetAssessmentById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var assessmentId = 999;

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync((Assessment?)null);

        // Act
        var result = await _assessmentService.GetAssessmentById(assessmentId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Assessment", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region GetAssessmentsByModuleId Tests

    [Fact]
    public async Task GetAssessmentsByModuleId_WithValidModuleId_ReturnsAssessments()
    {
        // Arrange
        var moduleId = 1;
        var assessments = new List<Assessment>
        {
            new Assessment
            {
                AssessmentId = 1,
                Title = "Assessment 1",
                ModuleId = moduleId
            },
            new Assessment
            {
                AssessmentId = 2,
                Title = "Assessment 2",
                ModuleId = moduleId
            }
        };

        var assessmentDtos = new List<AssessmentDto>
        {
            new AssessmentDto
            {
                AssessmentId = 1,
                Title = "Assessment 1",
                ModuleId = moduleId
            },
            new AssessmentDto
            {
                AssessmentId = 2,
                Title = "Assessment 2",
                ModuleId = moduleId
            }
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentsByModuleId(moduleId))
            .ReturnsAsync(assessments);

        _mapperMock
            .Setup(x => x.Map<List<AssessmentDto>>(assessments))
            .Returns(assessmentDtos);

        // Act
        var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
        Assert.Contains("Lấy danh sách Assessments thành công", result.Message);

        _assessmentRepositoryMock.Verify(x => x.GetAssessmentsByModuleId(moduleId), Times.Once);
    }

    [Fact]
    public async Task GetAssessmentsByModuleId_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var moduleId = 1;

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentsByModuleId(moduleId))
            .ReturnsAsync(new List<Assessment>());

        _mapperMock
            .Setup(x => x.Map<List<AssessmentDto>>(It.IsAny<List<Assessment>>()))
            .Returns(new List<AssessmentDto>());

        // Act
        var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region CreateAssessment Tests

    [Fact]
    public async Task CreateAssessment_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateAssessmentDto
        {
            ModuleId = 1,
            Title = "New Assessment",
            Description = "Test Description"
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            Title = dto.Title,
            Description = dto.Description,
            ModuleId = dto.ModuleId
        };

        var assessmentDto = new AssessmentDto
        {
            AssessmentId = 1,
            Title = dto.Title,
            Description = dto.Description,
            ModuleId = dto.ModuleId
        };

        _assessmentRepositoryMock
            .Setup(x => x.ModuleExists(dto.ModuleId))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map<Assessment>(dto))
            .Returns(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.AddAssessment(It.IsAny<Assessment>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<AssessmentDto>(It.IsAny<Assessment>()))
            .Returns(assessmentDto);

        // Act
        var result = await _assessmentService.CreateAssessment(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Tạo Assessment thành công", result.Message);

        _assessmentRepositoryMock.Verify(x => x.AddAssessment(It.IsAny<Assessment>()), Times.Once);
    }

    [Fact]
    public async Task CreateAssessment_WithNonExistentModule_ReturnsNotFound()
    {
        // Arrange
        var dto = new CreateAssessmentDto
        {
            ModuleId = 999,
            Title = "New Assessment"
        };

        _assessmentRepositoryMock
            .Setup(x => x.ModuleExists(dto.ModuleId))
            .ReturnsAsync(false);

        // Act
        var result = await _assessmentService.CreateAssessment(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Module", result.Message);

        _assessmentRepositoryMock.Verify(x => x.AddAssessment(It.IsAny<Assessment>()), Times.Never);
    }

    [Fact]
    public async Task CreateAssessment_AsTeacherWithOwnModule_ReturnsSuccess()
    {
        // Arrange
        var teacherId = 1;
        var dto = new CreateAssessmentDto
        {
            ModuleId = 1,
            Title = "New Assessment"
        };

        var assessment = new Assessment
        {
            AssessmentId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId
        };

        var assessmentDto = new AssessmentDto
        {
            AssessmentId = 1,
            Title = dto.Title,
            ModuleId = dto.ModuleId
        };

        _assessmentRepositoryMock
            .Setup(x => x.ModuleExists(dto.ModuleId))
            .ReturnsAsync(true);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfModule(teacherId, dto.ModuleId))
            .ReturnsAsync(true);

        _mapperMock
            .Setup(x => x.Map<Assessment>(dto))
            .Returns(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.AddAssessment(It.IsAny<Assessment>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<AssessmentDto>(It.IsAny<Assessment>()))
            .Returns(assessmentDto);

        // Act
        var result = await _assessmentService.CreateAssessment(dto, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task CreateAssessment_AsTeacherWithWrongModule_ReturnsForbidden()
    {
        // Arrange
        var teacherId = 1;
        var dto = new CreateAssessmentDto
        {
            ModuleId = 1,
            Title = "New Assessment"
        };

        _assessmentRepositoryMock
            .Setup(x => x.ModuleExists(dto.ModuleId))
            .ReturnsAsync(true);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfModule(teacherId, dto.ModuleId))
            .ReturnsAsync(false);

        // Act
        var result = await _assessmentService.CreateAssessment(dto, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền tạo Assessment", result.Message);

        _assessmentRepositoryMock.Verify(x => x.AddAssessment(It.IsAny<Assessment>()), Times.Never);
    }

    #endregion

    #region UpdateAssessment Tests

    [Fact]
    public async Task UpdateAssessment_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var assessmentId = 1;
        var dto = new UpdateAssessmentDto
        {
            Title = "Updated Assessment"
        };

        var existingAssessment = new Assessment
        {
            AssessmentId = assessmentId,
            Title = "Original Assessment",
            ModuleId = 1
        };

        var updatedAssessment = new Assessment
        {
            AssessmentId = assessmentId,
            Title = dto.Title,
            ModuleId = 1
        };

        var assessmentDto = new AssessmentDto
        {
            AssessmentId = assessmentId,
            Title = dto.Title,
            ModuleId = 1
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync(existingAssessment);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateAssessmentDto>(), It.IsAny<Assessment>()))
            .Returns(updatedAssessment);

        _assessmentRepositoryMock
            .Setup(x => x.UpdateAssessment(It.IsAny<Assessment>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<AssessmentDto>(It.IsAny<Assessment>()))
            .Returns(assessmentDto);

        // Act
        var result = await _assessmentService.UpdateAssessment(assessmentId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.Title, result.Data.Title);
        Assert.Contains("Cập nhật Assessment thành công", result.Message);

        _assessmentRepositoryMock.Verify(x => x.UpdateAssessment(It.IsAny<Assessment>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAssessment_WithNonExistentAssessment_ReturnsNotFound()
    {
        // Arrange
        var assessmentId = 999;
        var dto = new UpdateAssessmentDto
        {
            Title = "Updated Assessment"
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync((Assessment?)null);

        // Act
        var result = await _assessmentService.UpdateAssessment(assessmentId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Assessment", result.Message);

        _assessmentRepositoryMock.Verify(x => x.UpdateAssessment(It.IsAny<Assessment>()), Times.Never);
    }

    #endregion

    #region DeleteAssessment Tests

    [Fact]
    public async Task DeleteAssessment_WithValidAssessment_ReturnsSuccess()
    {
        // Arrange
        var assessmentId = 1;

        var assessment = new Assessment
        {
            AssessmentId = assessmentId,
            Title = "Test Assessment",
            ModuleId = 1
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.DeleteAssessment(assessmentId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _assessmentService.DeleteAssessment(assessmentId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Xóa Assessment thành công", result.Message);

        _assessmentRepositoryMock.Verify(x => x.DeleteAssessment(assessmentId), Times.Once);
    }

    [Fact]
    public async Task DeleteAssessment_WithNonExistentAssessment_ReturnsNotFound()
    {
        // Arrange
        var assessmentId = 999;

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync((Assessment?)null);

        // Act
        var result = await _assessmentService.DeleteAssessment(assessmentId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy Assessment", result.Message);

        _assessmentRepositoryMock.Verify(x => x.DeleteAssessment(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAssessment_AsTeacherWithOwnModule_ReturnsSuccess()
    {
        // Arrange
        var assessmentId = 1;
        var teacherId = 1;

        var assessment = new Assessment
        {
            AssessmentId = assessmentId,
            Title = "Test Assessment",
            ModuleId = 1
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId))
            .ReturnsAsync(true);

        _assessmentRepositoryMock
            .Setup(x => x.DeleteAssessment(assessmentId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _assessmentService.DeleteAssessment(assessmentId, teacherId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteAssessment_AsTeacherWithWrongModule_ReturnsForbidden()
    {
        // Arrange
        var assessmentId = 1;
        var teacherId = 1;

        var assessment = new Assessment
        {
            AssessmentId = assessmentId,
            Title = "Test Assessment",
            ModuleId = 1
        };

        _assessmentRepositoryMock
            .Setup(x => x.GetAssessmentById(assessmentId))
            .ReturnsAsync(assessment);

        _assessmentRepositoryMock
            .Setup(x => x.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId))
            .ReturnsAsync(false);

        // Act
        var result = await _assessmentService.DeleteAssessment(assessmentId, teacherId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Teacher không có quyền xóa Assessment", result.Message);

        _assessmentRepositoryMock.Verify(x => x.DeleteAssessment(It.IsAny<int>()), Times.Never);
    }

    #endregion
}

