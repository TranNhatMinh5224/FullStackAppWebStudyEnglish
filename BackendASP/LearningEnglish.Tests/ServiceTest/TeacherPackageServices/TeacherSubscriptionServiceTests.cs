using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.TeacherPackageServices;

public class TeacherSubscriptionServiceTests
{
    private readonly Mock<ITeacherSubscriptionRepository> _subscriptionRepoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<TeacherSubscriptionService>> _loggerMock;
    private readonly TeacherSubscriptionService _service;

    public TeacherSubscriptionServiceTests()
    {
        _subscriptionRepoMock = new Mock<ITeacherSubscriptionRepository>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<TeacherSubscriptionService>>();

        _service = new TeacherSubscriptionService(
            _subscriptionRepoMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task AddTeacherSubscriptionAsync_AlreadyHasActiveSubscription_ReturnsBadRequest()
    {
        // Arrange
        var userId = 1;
        var dto = new PurchaseTeacherPackageDto { IdTeacherPackage = 10 };
        var activeSub = new TeacherSubscription { EndDate = DateTime.UtcNow.AddMonths(1) };
        _subscriptionRepoMock.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync(activeSub);

        // Act
        var result = await _service.AddTeacherSubscriptionAsync(dto, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("đã có gói giáo viên đang hoạt động", result.Message);
    }

    [Fact]
    public async Task AddTeacherSubscriptionAsync_Success_CreatesNewSubscription()
    {
        // Arrange
        var userId = 1;
        var dto = new PurchaseTeacherPackageDto { IdTeacherPackage = 10 };
        _subscriptionRepoMock.Setup(r => r.GetActiveSubscriptionAsync(userId)).ReturnsAsync((TeacherSubscription)null);
        _mapperMock.Setup(m => m.Map<ResPurchaseTeacherPackageDto>(It.IsAny<TeacherSubscription>()))
            .Returns(new ResPurchaseTeacherPackageDto());

        // Act
        var result = await _service.AddTeacherSubscriptionAsync(dto, userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(201, result.StatusCode);
        _subscriptionRepoMock.Verify(r => r.AddTeacherSubscriptionAsync(It.Is<TeacherSubscription>(s => 
            s.UserId == userId && 
            s.TeacherPackageId == 10 && 
            s.Status == SubscriptionStatus.Active)), Times.Once);
    }

    [Fact]
    public async Task DeleteTeacherSubscriptionAsync_NotOwner_ReturnsNotFoundOrForbidden()
    {
        // Arrange
        var userId = 1;
        var subId = 100;
        _subscriptionRepoMock.Setup(r => r.GetTeacherSubscriptionByIdAndUserIdAsync(subId, userId))
            .ReturnsAsync((TeacherSubscription)null);

        // Act
        var result = await _service.DeleteTeacherSubscriptionAsync(new DeleteTeacherSubscriptionDto { TeacherSubscriptionId = subId }, userId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }
}
