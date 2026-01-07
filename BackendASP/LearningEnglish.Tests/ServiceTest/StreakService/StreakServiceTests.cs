using AutoMapper;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.StreakService;

public class StreakServiceTests
{
    private readonly Mock<IStreakRepository> _streakRepoMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<LearningEnglish.Application.Service.StreakService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly LearningEnglish.Application.Service.StreakService _service;

    public StreakServiceTests()
    {
        _streakRepoMock = new Mock<IStreakRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<LearningEnglish.Application.Service.StreakService>>();
        _mapperMock = new Mock<IMapper>();

        _service = new LearningEnglish.Application.Service.StreakService(
            _streakRepoMock.Object,
            _notificationRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task UpdateStreakAsync_NewUser_CreatesStreakOne()
    {
        // Arrange
        var userId = 1;
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync((Streak)null);

        // Act
        var result = await _service.UpdateStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.Data.NewCurrentStreak);
        _streakRepoMock.Verify(r => r.CreateAsync(It.Is<Streak>(s => s.CurrentStreak == 1)), Times.Once);
    }

    [Fact]
    public async Task UpdateStreakAsync_AlreadyUpdatedToday_ReturnsSuccessWithoutChange()
    {
        // Arrange
        var userId = 1;
        var today = DateTime.UtcNow.Date;
        var streak = new Streak { UserId = userId, CurrentStreak = 5, LastActivityDate = today };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        // Act
        var result = await _service.UpdateStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.Data.NewCurrentStreak); // Không tăng
        _streakRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Streak>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStreakAsync_ContinuousStreak_IncrementsStreak()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var streak = new Streak { UserId = userId, CurrentStreak = 5, LastActivityDate = yesterday };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        // Act
        var result = await _service.UpdateStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(6, result.Data.NewCurrentStreak); // Tăng lên 6
        _streakRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Streak>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStreakAsync_BrokenStreak_ResetsToOne()
    {
        // Arrange
        var userId = 1;
        var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);
        var streak = new Streak { UserId = userId, CurrentStreak = 10, LastActivityDate = twoDaysAgo };
        _streakRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(streak);

        // Act
        var result = await _service.UpdateStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.Data.NewCurrentStreak); // Reset về 1
        _streakRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Streak>()), Times.Once);
    }

    [Fact]
    public async Task SendStreakRemindersAsync_Success_SendsNotificationsAndEmails()
    {
        // Arrange
        var user = new User { UserId = 1, Email = "test@test.com", FirstName = "Test" };
        var streak = new Streak { UserId = 1, User = user, CurrentStreak = 5 };
        var usersAtRisk = new List<Streak> { streak };

        _streakRepoMock.Setup(r => r.GetUsersAtRiskOfLosingStreakAsync(3)).ReturnsAsync(usersAtRisk);

        // Act
        var result = await _service.SendStreakRemindersAsync();

        // Assert
        Assert.True(result.Success);
        _notificationRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendStreakReminderEmailAsync(user.Email, user.FullName, 5, It.IsAny<int>()), Times.Once);
    }
}
