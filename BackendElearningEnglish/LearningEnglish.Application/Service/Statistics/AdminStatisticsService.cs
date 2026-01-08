using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminStatisticsService : IAdminStatisticsService
    {
        private readonly IUserStatisticsRepository _userStatisticsRepository;
        private readonly IPaymentStatisticsRepository _paymentStatisticsRepository;
        private readonly ILogger<AdminStatisticsService> _logger;

        public AdminStatisticsService(
            IUserStatisticsRepository userStatisticsRepository,
            IPaymentStatisticsRepository paymentStatisticsRepository,
            ILogger<AdminStatisticsService> logger)
        {
            _userStatisticsRepository = userStatisticsRepository;
            _paymentStatisticsRepository = paymentStatisticsRepository;
            _logger = logger;
        }

        // Lấy tổng quan statistics cho dashboard
        public async Task<ServiceResponse<AdminOverviewStatisticsDto>> GetOverviewStatisticsAsync()
        {
            var response = new ServiceResponse<AdminOverviewStatisticsDto>();

            try
            {
                _logger.LogInformation("Fetching admin overview statistics");

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                // Sequential execution to avoid DbContext concurrency issues
                var totalUsers = await _userStatisticsRepository.GetTotalUsersCountAsync();
                var totalStudents = await _userStatisticsRepository.GetUserCountByRoleAsync("Student");
                var totalTeachers = await _userStatisticsRepository.GetUserCountByRoleAsync("Teacher");
                var totalAdmins = await _userStatisticsRepository.GetUserCountByRoleAsync("Admin");
                var newUsers = await _userStatisticsRepository.GetNewUsersCountAsync(thirtyDaysAgo);
                var totalRevenue = await _paymentStatisticsRepository.GetTotalRevenueAsync();

                var statistics = new AdminOverviewStatisticsDto
                {
                    TotalUsers = totalUsers,
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalAdmins = totalAdmins,
                    NewUsersLast30Days = newUsers,
                    TotalRevenue = totalRevenue
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê tổng quan thành công";

                _logger.LogInformation("Successfully fetched admin overview statistics");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thống kê tổng quan";
                _logger.LogError(ex, "Error fetching admin overview statistics");
            }

            return response;
        }

        // Lấy chi tiết thống kê users
        public async Task<ServiceResponse<UserStatisticsDto>> GetUserStatisticsAsync()
        {
            var response = new ServiceResponse<UserStatisticsDto>();

            try
            {
                _logger.LogInformation("Fetching user statistics");

                var now = DateTime.UtcNow;
                var today = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
                var weekAgo = now.AddDays(-7);
                var monthAgo = now.AddMonths(-1);

                // Sequential execution to avoid DbContext concurrency issues
                var totalUsers = await _userStatisticsRepository.GetTotalUsersCountAsync();
                var totalStudents = await _userStatisticsRepository.GetUserCountByRoleAsync("Student");
                var totalTeachers = await _userStatisticsRepository.GetUserCountByRoleAsync("Teacher");
                var totalAdmins = await _userStatisticsRepository.GetUserCountByRoleAsync("Admin");
                var activeUsers = await _userStatisticsRepository.GetActiveUsersCountAsync();
                var blockedUsers = await _userStatisticsRepository.GetBlockedUsersCountAsync();
                var newUsersToday = await _userStatisticsRepository.GetNewUsersCountAsync(today);
                var newUsersThisWeek = await _userStatisticsRepository.GetNewUsersCountAsync(weekAgo);
                var newUsersThisMonth = await _userStatisticsRepository.GetNewUsersCountAsync(monthAgo);

                var statistics = new UserStatisticsDto
                {
                    TotalUsers = totalUsers,
                    TotalStudents = totalStudents,
                    TotalTeachers = totalTeachers,
                    TotalAdmins = totalAdmins,
                    ActiveUsers = activeUsers,
                    BlockedUsers = blockedUsers,
                    NewUsersToday = newUsersToday,
                    NewUsersThisWeek = newUsersThisWeek,
                    NewUsersThisMonth = newUsersThisMonth
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê users thành công";

                _logger.LogInformation("Successfully fetched user statistics");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thống kê users";
                _logger.LogError(ex, "Error fetching user statistics");
            }

            return response;
        }

        // Lấy chi tiết thống kê doanh thu
        public async Task<ServiceResponse<RevenueStatisticsDto>> GetRevenueStatisticsAsync()
        {
            var response = new ServiceResponse<RevenueStatisticsDto>();

            try
            {
                _logger.LogInformation("Fetching revenue statistics");

                var now = DateTime.UtcNow;
                var today = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
                var startOfWeek = DateTime.SpecifyKind(today.AddDays(-(int)today.DayOfWeek), DateTimeKind.Utc);
                var startOfMonth = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
                var startOfYear = DateTime.SpecifyKind(new DateTime(now.Year, 1, 1), DateTimeKind.Utc);

                // Sequential execution to avoid DbContext concurrency issues
                var totalRevenue = await _paymentStatisticsRepository.GetTotalRevenueAsync();
                var completedRevenue = await _paymentStatisticsRepository.GetRevenueByStatusAsync(PaymentStatus.Completed);
                var pendingRevenue = await _paymentStatisticsRepository.GetRevenueByStatusAsync(PaymentStatus.Pending);
                
                var revenueToday = await _paymentStatisticsRepository.GetRevenueByDateRangeAsync(today);
                var revenueWeek = await _paymentStatisticsRepository.GetRevenueByDateRangeAsync(startOfWeek);
                var revenueMonth = await _paymentStatisticsRepository.GetRevenueByDateRangeAsync(startOfMonth);
                var revenueYear = await _paymentStatisticsRepository.GetRevenueByDateRangeAsync(startOfYear);
                
                var totalTransactions = await _paymentStatisticsRepository.GetTotalTransactionsCountAsync();
                var completedTransactions = await _paymentStatisticsRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Completed);
                var pendingTransactions = await _paymentStatisticsRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Pending);
                var failedTransactions = await _paymentStatisticsRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Failed);
                
                var transactionsToday = await _paymentStatisticsRepository.GetTransactionsCountByDateRangeAsync(today);
                var transactionsWeek = await _paymentStatisticsRepository.GetTransactionsCountByDateRangeAsync(startOfWeek);
                var transactionsMonth = await _paymentStatisticsRepository.GetTransactionsCountByDateRangeAsync(startOfMonth);

                var statistics = new RevenueStatisticsDto
                {
                    TotalRevenue = totalRevenue,
                    CompletedRevenue = completedRevenue,
                    PendingRevenue = pendingRevenue,
                    
                    RevenueToday = revenueToday,
                    RevenueThisWeek = revenueWeek,
                    RevenueThisMonth = revenueMonth,
                    RevenueThisYear = revenueYear,
                    
                    TotalTransactions = totalTransactions,
                    CompletedTransactions = completedTransactions,
                    PendingTransactions = pendingTransactions,
                    FailedTransactions = failedTransactions,
                    
                    AverageTransactionValue = completedTransactions > 0 
                        ? totalRevenue / completedTransactions 
                        : 0,
                    
                    TransactionsToday = transactionsToday,
                    TransactionsThisWeek = transactionsWeek,
                    TransactionsThisMonth = transactionsMonth
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê doanh thu thành công";

                _logger.LogInformation("Successfully fetched revenue statistics");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thống kê doanh thu";
                _logger.LogError(ex, "Error fetching revenue statistics");
            }

            return response;
        }

        // Lấy dữ liệu doanh thu cho biểu đồ
        // Note: Validation được xử lý bởi FluentValidation (GetRevenueChartDataRequestDtoValidator)
        public async Task<ServiceResponse<RevenueChartDto>> GetRevenueChartDataAsync(int days = 30)
        {
            var response = new ServiceResponse<RevenueChartDto>();

            try
            {
                _logger.LogInformation("📊 Fetching revenue chart data for last {Days} days", days);

                var now = DateTime.UtcNow;
                var fromDate = DateTime.SpecifyKind(now.AddDays(-days).Date, DateTimeKind.Utc);
                var toDate = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc);
                var currentYear = now.Year;

                // Sequential execution to avoid DbContext concurrency issues
                var totalRevenue = await _paymentStatisticsRepository.GetTotalRevenueAsync();
                var courseRevenue = await _paymentStatisticsRepository.GetRevenueByProductTypeAsync(ProductType.Course);
                var teacherPackageRevenue = await _paymentStatisticsRepository.GetRevenueByProductTypeAsync(ProductType.TeacherPackage);
                var dailyRevenue = await _paymentStatisticsRepository.GetDailyRevenueAsync(fromDate, toDate);
                var monthlyRevenue = await _paymentStatisticsRepository.GetMonthlyRevenueAsync(currentYear);

                // Get daily revenue breakdown by ProductType
                var dailyCourseRevenue = await _paymentStatisticsRepository.GetDailyRevenueByProductTypeAsync(ProductType.Course, fromDate, toDate);
                var dailyTeacherPackageRevenue = await _paymentStatisticsRepository.GetDailyRevenueByProductTypeAsync(ProductType.TeacherPackage, fromDate, toDate);

                // Format data for chart
                var chartData = new RevenueChartDto
                {
                    TotalRevenue = totalRevenue,
                    CourseRevenue = courseRevenue,
                    TeacherPackageRevenue = teacherPackageRevenue,
                    
                    // Daily revenue timeline (fill missing dates with 0)
                    DailyRevenue = Enumerable.Range(0, days + 1)
                        .Select(offset => fromDate.AddDays(offset))
                        .Select(date => new RevenueTimelineItem
                        {
                            Date = date,
                            Amount = dailyRevenue.GetValueOrDefault(date, 0)
                        })
                        .ToList(),
                    
                    // Monthly revenue timeline for current year
                    MonthlyRevenue = Enumerable.Range(1, 12)
                        .Select(month => DateTime.SpecifyKind(new DateTime(currentYear, month, 1), DateTimeKind.Utc))
                        .Select(date => new RevenueTimelineItem
                        {
                            Date = date,
                            Amount = monthlyRevenue.GetValueOrDefault(date, 0)
                        })
                        .ToList(),

                    // Daily revenue breakdown by ProductType
                    DailyCourseRevenue = Enumerable.Range(0, days + 1)
                        .Select(offset => fromDate.AddDays(offset))
                        .Select(date => new RevenueTimelineItem
                        {
                            Date = date,
                            Amount = dailyCourseRevenue.GetValueOrDefault(date, 0)
                        })
                        .ToList(),

                    DailyTeacherPackageRevenue = Enumerable.Range(0, days + 1)
                        .Select(offset => fromDate.AddDays(offset))
                        .Select(date => new RevenueTimelineItem
                        {
                            Date = date,
                            Amount = dailyTeacherPackageRevenue.GetValueOrDefault(date, 0)
                        })
                        .ToList()
                };

                response.Data = chartData;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy dữ liệu biểu đồ doanh thu thành công";

                _logger.LogInformation("✅ Successfully fetched revenue chart data: Total={Total}, Course={Course}, TeacherPackage={TeacherPackage}",
                    chartData.TotalRevenue, chartData.CourseRevenue, chartData.TeacherPackageRevenue);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy dữ liệu biểu đồ doanh thu";
                _logger.LogError(ex, "❌ Error fetching revenue chart data");
            }

            return response;
        }
    }
}
