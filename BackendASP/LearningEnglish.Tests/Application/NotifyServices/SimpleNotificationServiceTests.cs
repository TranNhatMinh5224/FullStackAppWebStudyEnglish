using LearningEnglish.Application.Service;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Moq;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Tests.Application.NotifyServices;

public class SimpleNotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<ILogger<SimpleNotificationService>> _loggerMock;
    private readonly SimpleNotificationService _simpleNotificationService;

    public SimpleNotificationServiceTests()
    {
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _loggerMock = new Mock<ILogger<SimpleNotificationService>>();

        _simpleNotificationService = new SimpleNotificationService(
            _notificationRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    #region CreateNotificationAsync Tests

    [Fact]
    public async Task CreateNotificationAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var title = "Test Notification";
        var message = "This is a test notification";
        var type = NotificationType.CourseEnrollment;

        _notificationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _simpleNotificationService.CreateNotificationAsync(
            userId, title, message, type);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(200, result.StatusCode);
        Assert.True(result.Data);
        Assert.Contains("Tạo notification thành công", result.Message);

        _notificationRepositoryMock.Verify(x => x.AddAsync(It.Is<Notification>(n =>
            n.UserId == userId &&
            n.Title == title &&
            n.Message == message &&
            n.Type == type &&
            n.IsRead == false)), Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithRelatedEntity_ReturnsSuccess()
    {
        // Arrange
        var userId = 1;
        var title = "Test Notification";
        var message = "This is a test notification";
        var type = NotificationType.AssessmentGraded;
        var relatedEntityType = "Course";
        var relatedEntityId = 1;

        _notificationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _simpleNotificationService.CreateNotificationAsync(
            userId, title, message, type, relatedEntityType, relatedEntityId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Data);

        _notificationRepositoryMock.Verify(x => x.AddAsync(It.Is<Notification>(n =>
            n.RelatedEntityType == relatedEntityType &&
            n.RelatedEntityId == relatedEntityId)), Times.Once);
    }

    [Fact]
    public async Task CreateNotificationAsync_WithException_ReturnsError()
    {
        // Arrange
        var userId = 1;
        var title = "Test Notification";
        var message = "This is a test notification";
        var type = NotificationType.CourseEnrollment;

        _notificationRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Notification>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _simpleNotificationService.CreateNotificationAsync(
            userId, title, message, type);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(500, result.StatusCode);
        Assert.Contains("Lỗi khi tạo notification", result.Message);
    }

    #endregion
}

