using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.TeacherPackageServices;

public class TeacherPackageServiceTests
{
    private readonly Mock<ITeacherPackageRepository> _teacherPackageRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TeacherPackageService>> _loggerMock;
    private readonly TeacherPackageService _service;

    public TeacherPackageServiceTests()
    {
        _teacherPackageRepositoryMock = new Mock<ITeacherPackageRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TeacherPackageService>>();

        _service = new TeacherPackageService(
            _teacherPackageRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetAllTeacherPackagesAsync_Success_ReturnsList()
    {
        // Arrange
        var packages = new List<TeacherPackage> { new TeacherPackage { TeacherPackageId = 1 } };
        _teacherPackageRepositoryMock.Setup(r => r.GetAllTeacherPackagesAsync()).ReturnsAsync(packages);
        _mapperMock.Setup(m => m.Map<List<TeacherPackageDto>>(packages)).Returns(new List<TeacherPackageDto>());

        // Act
        var result = await _service.GetAllTeacherPackagesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task CreateTeacherPackageAsync_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateTeacherPackageDto { PackageName = "Basic" };
        var existing = new List<TeacherPackage> { new TeacherPackage { PackageName = "Basic" } };
        _teacherPackageRepositoryMock.Setup(r => r.GetAllTeacherPackagesAsync()).ReturnsAsync(existing);

        // Act
        var result = await _service.CreateTeacherPackageAsync(dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Gói giáo viên với tên này đã tồn tại", result.Message);
    }

    [Fact]
    public async Task CreateTeacherPackageAsync_Success_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateTeacherPackageDto { PackageName = "Premium" };
        _teacherPackageRepositoryMock.Setup(r => r.GetAllTeacherPackagesAsync()).ReturnsAsync(new List<TeacherPackage>());
        _mapperMock.Setup(m => m.Map<TeacherPackage>(dto)).Returns(new TeacherPackage());
        _mapperMock.Setup(m => m.Map<TeacherPackageDto>(It.IsAny<TeacherPackage>())).Returns(new TeacherPackageDto());

        // Act
        var result = await _service.CreateTeacherPackageAsync(dto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _teacherPackageRepositoryMock.Verify(r => r.AddTeacherPackageAsync(It.IsAny<TeacherPackage>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTeacherPackageAsync_DuplicateName_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        var dto = new UpdateTeacherPackageDto { PackageName = "Gold" };
        var current = new TeacherPackage { TeacherPackageId = 1, PackageName = "Silver" };
        var allPackages = new List<TeacherPackage> 
        { 
            current, 
            new TeacherPackage { TeacherPackageId = 2, PackageName = "Gold" } // Duplicate exists
        };

        _teacherPackageRepositoryMock.Setup(r => r.GetTeacherPackageByIdAsync(id)).ReturnsAsync(current);
        _teacherPackageRepositoryMock.Setup(r => r.GetAllTeacherPackagesAsync()).ReturnsAsync(allPackages);

        // Act
        var result = await _service.UpdateTeacherPackageAsync(id, dto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task DeleteTeacherPackageAsync_HasActiveSubscriptions_ReturnsBadRequest()
    {
        // Arrange
        var id = 1;
        _teacherPackageRepositoryMock.Setup(r => r.GetTeacherPackageByIdAsync(id)).ReturnsAsync(new TeacherPackage());
        _teacherPackageRepositoryMock.Setup(r => r.HasActiveSubscriptionsAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteTeacherPackageAsync(id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đang được sử dụng", result.Message);
    }

    [Fact]
    public async Task DeleteTeacherPackageAsync_Success_DeletesPackage()
    {
        // Arrange
        var id = 1;
        _teacherPackageRepositoryMock.Setup(r => r.GetTeacherPackageByIdAsync(id)).ReturnsAsync(new TeacherPackage());
        _teacherPackageRepositoryMock.Setup(r => r.HasActiveSubscriptionsAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _service.DeleteTeacherPackageAsync(id);

        // Assert
        Assert.True(result.Success);
        _teacherPackageRepositoryMock.Verify(r => r.DeleteTeacherPackageAsync(id), Times.Once);
    }
}
