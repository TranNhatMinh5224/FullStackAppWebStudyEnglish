using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Tests.Application.TeacherPackageServices;

public class TeacherPackageServiceTests
{
    private readonly Mock<ITeacherPackageRepository> _teacherPackageRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TeacherPackageService>> _loggerMock;
    private readonly TeacherPackageService _teacherPackageService;

    public TeacherPackageServiceTests()
    {
        _teacherPackageRepositoryMock = new Mock<ITeacherPackageRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TeacherPackageService>>();

        _teacherPackageService = new TeacherPackageService(
            _teacherPackageRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    #region GetAllTeacherPackagesAsync Tests

    [Fact]
    public async Task GetAllTeacherPackagesAsync_WithExistingPackages_ReturnsPackages()
    {
        // Arrange
        var packages = new List<TeacherPackage>
        {
            new TeacherPackage
            {
                TeacherPackageId = 1,
                PackageName = "Basic",
                MaxLessons = 10,
                MaxCourses = 5
            },
            new TeacherPackage
            {
                TeacherPackageId = 2,
                PackageName = "Premium",
                MaxLessons = 50,
                MaxCourses = 20
            }
        };

        var packageDtos = new List<TeacherPackageDto>
        {
            new TeacherPackageDto
            {
                TeacherPackageId = 1,
                PackageName = "Basic",
                MaxLessons = 10,
                MaxCourses = 5
            },
            new TeacherPackageDto
            {
                TeacherPackageId = 2,
                PackageName = "Premium",
                MaxLessons = 50,
                MaxCourses = 20
            }
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetAllTeacherPackagesAsync())
            .ReturnsAsync(packages);

        _mapperMock
            .Setup(x => x.Map<List<TeacherPackageDto>>(packages))
            .Returns(packageDtos);

        // Act
        var result = await _teacherPackageService.GetAllTeacherPackagesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);

        _teacherPackageRepositoryMock.Verify(x => x.GetAllTeacherPackagesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetAllTeacherPackagesAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        _teacherPackageRepositoryMock
            .Setup(x => x.GetAllTeacherPackagesAsync())
            .ReturnsAsync(new List<TeacherPackage>());

        _mapperMock
            .Setup(x => x.Map<List<TeacherPackageDto>>(It.IsAny<List<TeacherPackage>>()))
            .Returns(new List<TeacherPackageDto>());

        // Act
        var result = await _teacherPackageService.GetAllTeacherPackagesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    #endregion

    #region GetTeacherPackageByIdAsync Tests

    [Fact]
    public async Task GetTeacherPackageByIdAsync_WithValidId_ReturnsPackage()
    {
        // Arrange
        var packageId = 1;
        var package = new TeacherPackage
        {
            TeacherPackageId = packageId,
            PackageName = "Basic",
            MaxLessons = 10,
            MaxCourses = 5
        };

        var packageDto = new TeacherPackageDto
        {
            TeacherPackageId = packageId,
            PackageName = "Basic",
            MaxLessons = 10,
            MaxCourses = 5
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync(package);

        _mapperMock
            .Setup(x => x.Map<TeacherPackageDto>(package))
            .Returns(packageDto);

        // Act
        var result = await _teacherPackageService.GetTeacherPackageByIdAsync(packageId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(packageId, result.Data.TeacherPackageId);
        Assert.Equal("Basic", result.Data.PackageName);

        _teacherPackageRepositoryMock.Verify(x => x.GetTeacherPackageByIdAsync(packageId), Times.Once);
    }

    [Fact]
    public async Task GetTeacherPackageByIdAsync_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var packageId = 999;

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync((TeacherPackage?)null);

        // Act
        var result = await _teacherPackageService.GetTeacherPackageByIdAsync(packageId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy gói giáo viên", result.Message);
        Assert.Null(result.Data);
    }

    #endregion

    #region CreateTeacherPackageAsync Tests

    [Fact]
    public async Task CreateTeacherPackageAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateTeacherPackageDto
        {
            PackageName = "New Package",
            MaxLessons = 20,
            MaxCourses = 10,
            Price = 100000
        };

        var package = new TeacherPackage
        {
            TeacherPackageId = 1,
            PackageName = dto.PackageName,
            MaxLessons = dto.MaxLessons,
            MaxCourses = dto.MaxCourses,
            Price = dto.Price
        };

        var packageDto = new TeacherPackageDto
        {
            TeacherPackageId = 1,
            PackageName = dto.PackageName,
            MaxLessons = dto.MaxLessons,
            MaxCourses = dto.MaxCourses,
            Price = dto.Price
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetAllTeacherPackagesAsync())
            .ReturnsAsync(new List<TeacherPackage>());

        _mapperMock
            .Setup(x => x.Map<TeacherPackage>(dto))
            .Returns(package);

        _teacherPackageRepositoryMock
            .Setup(x => x.AddTeacherPackageAsync(It.IsAny<TeacherPackage>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<TeacherPackageDto>(It.IsAny<TeacherPackage>()))
            .Returns(packageDto);

        // Act
        var result = await _teacherPackageService.CreateTeacherPackageAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.PackageName, result.Data.PackageName);

        _teacherPackageRepositoryMock.Verify(x => x.AddTeacherPackageAsync(It.IsAny<TeacherPackage>()), Times.Once);
    }

    [Fact]
    public async Task CreateTeacherPackageAsync_WithDuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateTeacherPackageDto
        {
            PackageName = "Existing Package",
            MaxLessons = 20,
            MaxCourses = 10
        };

        var existingPackages = new List<TeacherPackage>
        {
            new TeacherPackage
            {
                TeacherPackageId = 1,
                PackageName = "Existing Package"
            }
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetAllTeacherPackagesAsync())
            .ReturnsAsync(existingPackages);

        // Act
        var result = await _teacherPackageService.CreateTeacherPackageAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Gói giáo viên với tên này đã tồn tại", result.Message);

        _teacherPackageRepositoryMock.Verify(x => x.AddTeacherPackageAsync(It.IsAny<TeacherPackage>()), Times.Never);
    }

    #endregion

    #region UpdateTeacherPackageAsync Tests

    [Fact]
    public async Task UpdateTeacherPackageAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var packageId = 1;
        var dto = new UpdateTeacherPackageDto
        {
            PackageName = "Updated Package",
            MaxLessons = 30,
            MaxCourses = 15
        };

        var existingPackage = new TeacherPackage
        {
            TeacherPackageId = packageId,
            PackageName = "Original Package",
            MaxLessons = 20,
            MaxCourses = 10
        };

        var updatedPackage = new TeacherPackage
        {
            TeacherPackageId = packageId,
            PackageName = dto.PackageName!,
            MaxLessons = dto.MaxLessons!.Value,
            MaxCourses = dto.MaxCourses!.Value
        };

        var packageDto = new TeacherPackageDto
        {
            TeacherPackageId = packageId,
            PackageName = dto.PackageName!,
            MaxLessons = dto.MaxLessons!.Value,
            MaxCourses = dto.MaxCourses!.Value
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync(existingPackage);

        _mapperMock
            .Setup(x => x.Map(It.IsAny<UpdateTeacherPackageDto>(), It.IsAny<TeacherPackage>()))
            .Returns(updatedPackage);

        _teacherPackageRepositoryMock
            .Setup(x => x.UpdateTeacherPackageAsync(It.IsAny<TeacherPackage>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(x => x.Map<TeacherPackageDto>(It.IsAny<TeacherPackage>()))
            .Returns(packageDto);

        // Act
        var result = await _teacherPackageService.UpdateTeacherPackageAsync(packageId, dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Data);
        Assert.Equal(dto.PackageName, result.Data.PackageName);

        _teacherPackageRepositoryMock.Verify(x => x.UpdateTeacherPackageAsync(It.IsAny<TeacherPackage>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeacherPackageAsync_WithNonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        var packageId = 999;
        var dto = new UpdateTeacherPackageDto
        {
            PackageName = "Updated Package"
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync((TeacherPackage?)null);

        // Act
        var result = await _teacherPackageService.UpdateTeacherPackageAsync(packageId, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy gói giáo viên", result.Message);

        _teacherPackageRepositoryMock.Verify(x => x.UpdateTeacherPackageAsync(It.IsAny<TeacherPackage>()), Times.Never);
    }

    #endregion

    #region DeleteTeacherPackageAsync Tests

    [Fact]
    public async Task DeleteTeacherPackageAsync_WithValidPackage_ReturnsSuccess()
    {
        // Arrange
        var packageId = 1;

        var package = new TeacherPackage
        {
            TeacherPackageId = packageId,
            PackageName = "Test Package"
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync(package);

        _teacherPackageRepositoryMock
            .Setup(x => x.DeleteTeacherPackageAsync(packageId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _teacherPackageService.DeleteTeacherPackageAsync(packageId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Xóa gói giáo viên thành công", result.Message);

        _teacherPackageRepositoryMock.Verify(x => x.DeleteTeacherPackageAsync(packageId), Times.Once);
    }

    [Fact]
    public async Task DeleteTeacherPackageAsync_WithNonExistentPackage_ReturnsNotFound()
    {
        // Arrange
        var packageId = 999;

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync((TeacherPackage?)null);

        // Act
        var result = await _teacherPackageService.DeleteTeacherPackageAsync(packageId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Contains("Không tìm thấy gói giáo viên", result.Message);

        _teacherPackageRepositoryMock.Verify(x => x.DeleteTeacherPackageAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteTeacherPackageAsync_WithDeleteFailure_ReturnsError()
    {
        // Arrange
        var packageId = 1;

        var package = new TeacherPackage
        {
            TeacherPackageId = packageId,
            PackageName = "Test Package"
        };

        _teacherPackageRepositoryMock
            .Setup(x => x.GetTeacherPackageByIdAsync(packageId))
            .ReturnsAsync(package);

        _teacherPackageRepositoryMock
            .Setup(x => x.DeleteTeacherPackageAsync(packageId))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _teacherPackageService.DeleteTeacherPackageAsync(packageId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Đã xảy ra lỗi khi xóa gói giáo viên", result.Message);
    }

    #endregion
}

