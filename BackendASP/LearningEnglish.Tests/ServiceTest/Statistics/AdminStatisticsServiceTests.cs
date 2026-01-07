using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Service;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LearningEnglish.Tests.ServiceTest.Statistics;

public class AdminStatisticsServiceTests
{
    private readonly Mock<IUserStatisticsRepository> _userStatsRepoMock;
    private readonly Mock<IPaymentStatisticsRepository> _paymentStatsRepoMock;
    private readonly Mock<ILogger<AdminStatisticsService>> _loggerMock;
    private readonly AdminStatisticsService _service;

    public AdminStatisticsServiceTests()
    {
        _userStatsRepoMock = new Mock<IUserStatisticsRepository>();
        _paymentStatsRepoMock = new Mock<IPaymentStatisticsRepository>();
        _loggerMock = new Mock<ILogger<AdminStatisticsService>>();

        _service = new AdminStatisticsService(
            _userStatsRepoMock.Object,
            _paymentStatsRepoMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task GetOverviewStatisticsAsync_Success_ReturnsAggregatedData()
    {
        // Arrange
        _userStatsRepoMock.Setup(r => r.GetTotalUsersCountAsync()).ReturnsAsync(100);
        _userStatsRepoMock.Setup(r => r.GetUserCountByRoleAsync("Student")).ReturnsAsync(80);
        _userStatsRepoMock.Setup(r => r.GetUserCountByRoleAsync("Teacher")).ReturnsAsync(15);
        _userStatsRepoMock.Setup(r => r.GetNewUsersCountAsync(It.IsAny<DateTime>())).ReturnsAsync(10);
        _paymentStatsRepoMock.Setup(r => r.GetTotalRevenueAsync()).ReturnsAsync(5000000m);

        // Act
        var result = await _service.GetOverviewStatisticsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(100, result.Data.TotalUsers);
        Assert.Equal(80, result.Data.TotalStudents);
        Assert.Equal(5000000m, result.Data.TotalRevenue);
    }

    [Fact]
    public async Task GetRevenueChartDataAsync_FillsMissingDatesWithZero()
    {
        // Arrange
        int days = 7;
        var now = DateTime.UtcNow.Date;
        var fromDate = now.AddDays(-days);
        
        // Chỉ mock 1 ngày có doanh thu
        var dailyRevenue = new Dictionary<DateTime, decimal>
        {
            { now, 1000m }
        };

        _paymentStatsRepoMock.Setup(r => r.GetDailyRevenueAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(dailyRevenue);
        _paymentStatsRepoMock.Setup(r => r.GetMonthlyRevenueAsync(It.IsAny<int>()))
            .ReturnsAsync(new Dictionary<DateTime, decimal>());

        // Act
        var result = await _service.GetRevenueChartDataAsync(days);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(days + 1, result.Data.DailyRevenue.Count);
        
        // Kiểm tra ngày 'now' có 1000, các ngày khác có 0
        var todayItem = result.Data.DailyRevenue.First(d => d.Date == now);
        Assert.Equal(1000m, todayItem.Amount);
        
        var yesterdayItem = result.Data.DailyRevenue.First(d => d.Date == now.AddDays(-1));
        Assert.Equal(0, yesterdayItem.Amount);
    }

    [Fact]
    public async Task GetUserStatisticsAsync_Success_ReturnsRoleBreakdown()
    {
        // Arrange
        _userStatsRepoMock.Setup(r => r.GetTotalUsersCountAsync()).ReturnsAsync(100);
        _userStatsRepoMock.Setup(r => r.GetActiveUsersCountAsync()).ReturnsAsync(90);
        _userStatsRepoMock.Setup(r => r.GetBlockedUsersCountAsync()).ReturnsAsync(10);

        // Act
        var result = await _service.GetUserStatisticsAsync();

        // Assert
        Assert.True(result.Success);
        Assert.Equal(100, result.Data.TotalUsers);
        Assert.Equal(90, result.Data.ActiveUsers);
        Assert.Equal(10, result.Data.BlockedUsers);
    }
}
