using LearningEnglish.Application.Service;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Tests.Application.StreakServices;

public class StreakServiceTests
{
    private readonly Mock<IStreakRepository> _streakRepositoryMock;
    private readonly Mock<INotificationRepository> _notificationRepositoryMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<ILogger<StreakService>> _loggerMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly StreakService _streakService;

    public StreakServiceTests()
    {
        _streakRepositoryMock = new Mock<IStreakRepository>();
        _notificationRepositoryMock = new Mock<INotificationRepository>();
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<StreakService>>();
        _mapperMock = new Mock<IMapper>();

        _streakService = new StreakService(
            _streakRepositoryMock.Object,
            _notificationRepositoryMock.Object,
            _emailServiceMock.Object,
            _loggerMock.Object,
            _mapperMock.Object
        );
    }

    #region GetCurrentStreakAsync Tests

    [Fact]
    public async Task GetCurrentStreakAsync_WithExistingStreak_ReturnsStreak()
    {
        // Arrange
        var userId = 1;
        var streak = new Streak
        {
            UserId = userId,
            CurrentStreak = 5,
            LongestStreak = 10,
            TotalActiveDays = 15,
            LastActivityDate = DateTime.UtcNow.Date
        };

        var streakDto = new StreakDto
        {
            CurrentStreak = 5
        };

        _streakRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(streak);

        _mapperMock
            .Setup(x => x.Map<StreakDto>(streak))
            .Returns(streakDto);

        // Act
        var result = await _streakService.GetCurrentStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(5, result.Data.CurrentStreak);
        Assert.Contains("Lấy streak thành công", result.Message);

        _streakRepositoryMock.Verify(x => x.GetByUserIdAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_WithNonExistentStreak_CreatesNewStreak()
    {
        // Arrange
        var userId = 1;

        var newStreak = new Streak
        {
            UserId = userId,
            CurrentStreak = 0,
            LongestStreak = 0,
            TotalActiveDays = 0
        };

        var streakDto = new StreakDto
        {
            CurrentStreak = 0
        };

        _streakRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Streak?)null);

        _streakRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Streak>()))
            .ReturnsAsync(newStreak);

        _mapperMock
            .Setup(x => x.Map<StreakDto>(It.IsAny<Streak>()))
            .Returns(streakDto);

        // Act
        var result = await _streakService.GetCurrentStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(0, result.Data.CurrentStreak);

        _streakRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Streak>()), Times.Once);
    }

    #endregion

    #region UpdateStreakAsync Tests

    [Fact]
    public async Task UpdateStreakAsync_WithNewRecord_UpdatesLongestStreak()
    {
        // Arrange
        var userId = 1;
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);

        var streak = new Streak
        {
            UserId = userId,
            CurrentStreak = 10,
            LongestStreak = 10,
            TotalActiveDays = 20,
            LastActivityDate = yesterday
        };

        var updatedStreak = new Streak
        {
            UserId = userId,
            CurrentStreak = 11,
            LongestStreak = 11, // New record
            TotalActiveDays = 21,
            LastActivityDate = DateTime.UtcNow.Date
        };

        var resultDto = new StreakUpdateResultDto
        {
            NewCurrentStreak = 11,
            NewLongestStreak = 11,
            IsNewRecord = true,
            Message = "New record!"
        };

        _streakRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync(streak);

        _streakRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Streak>()))
            .Returns(Task.CompletedTask);

        // StreakService builds DTO directly, no mapper needed

        // Act
        var result = await _streakService.UpdateStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.IsNewRecord);
        Assert.Equal(11, result.Data.NewCurrentStreak);
    }

    [Fact]
    public async Task UpdateStreakAsync_WithNonExistentStreak_CreatesNewStreak()
    {
        // Arrange
        var userId = 1;

        var newStreak = new Streak
        {
            UserId = userId,
            CurrentStreak = 1,
            LongestStreak = 1,
            TotalActiveDays = 1,
            LastActivityDate = DateTime.UtcNow.Date
        };

        var resultDto = new StreakUpdateResultDto
        {
            NewCurrentStreak = 1,
            NewLongestStreak = 1,
            IsNewRecord = false,
            Message = "Streak started"
        };

        _streakRepositoryMock
            .Setup(x => x.GetByUserIdAsync(userId))
            .ReturnsAsync((Streak?)null);

        _streakRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Streak>()))
            .ReturnsAsync(newStreak);

        // StreakService builds DTO directly, no mapper needed

        // Act
        var result = await _streakService.UpdateStreakAsync(userId);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data.NewCurrentStreak);

        _streakRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Streak>()), Times.Once);
    }

    #endregion
}

