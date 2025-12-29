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

                // Parallel execution để tối ưu performance - chỉ lấy User và Revenue
                var totalUsersTask = _userStatisticsRepository.GetTotalUsersCountAsync();
                var totalStudentsTask = _userStatisticsRepository.GetUserCountByRoleAsync("Student");
                var totalTeachersTask = _userStatisticsRepository.GetUserCountByRoleAsync("Teacher");
                var totalAdminsTask = _userStatisticsRepository.GetUserCountByRoleAsync("Admin");
                var newUsersTask = _userStatisticsRepository.GetNewUsersCountAsync(thirtyDaysAgo);
                var totalRevenueTask = _paymentStatisticsRepository.GetTotalRevenueAsync();

                await Task.WhenAll(totalUsersTask, totalStudentsTask, totalTeachersTask, totalAdminsTask, newUsersTask, totalRevenueTask);

                var totalUsers = await totalUsersTask;
                var totalStudents = await totalStudentsTask;
                var totalTeachers = await totalTeachersTask;
                var totalAdmins = await totalAdminsTask;
                var newUsers = await newUsersTask;
                var totalRevenue = await totalRevenueTask;

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
                var today = now.Date;
                var weekAgo = now.AddDays(-7);
                var monthAgo = now.AddMonths(-1);

                // Parallel execution
                var tasks = new[]
                {
                    _userStatisticsRepository.GetTotalUsersCountAsync(),
                    _userStatisticsRepository.GetUserCountByRoleAsync("Student"),
                    _userStatisticsRepository.GetUserCountByRoleAsync("Teacher"),
                    _userStatisticsRepository.GetUserCountByRoleAsync("Admin"),
                    _userStatisticsRepository.GetActiveUsersCountAsync(),
                    _userStatisticsRepository.GetBlockedUsersCountAsync(),
                    _userStatisticsRepository.GetNewUsersCountAsync(today),
                    _userStatisticsRepository.GetNewUsersCountAsync(weekAgo),
                    _userStatisticsRepository.GetNewUsersCountAsync(monthAgo)
                };

                var results = await Task.WhenAll(tasks);

                var statistics = new UserStatisticsDto
                {
                    TotalUsers = results[0],
                    TotalStudents = results[1],
                    TotalTeachers = results[2],
                    TotalAdmins = results[3],
                    ActiveUsers = results[4],
                    BlockedUsers = results[5],
                    NewUsersToday = results[6],
                    NewUsersThisWeek = results[7],
                    NewUsersThisMonth = results[8]
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
                var today = now.Date;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfYear = new DateTime(now.Year, 1, 1);

                // Parallel execution
                var revenueTask = _paymentStatisticsRepository.GetTotalRevenueAsync();
                var completedRevenueTask = _paymentStatisticsRepository.GetRevenueByStatusAsync(PaymentStatus.Completed);
                var pendingRevenueTask = _paymentStatisticsRepository.GetRevenueByStatusAsync(PaymentStatus.Pending);
                
                var revenueTodayTask = _paymentStatisticsRepository.GetRevenueByDateRangeAsync(today);
                var revenueWeekTask = _paymentStatisticsRepository.GetRevenueByDateRangeAsync(startOfWeek);
                var revenueMonthTask = _paymentStatisticsRepository.GetRevenueByDateRangeAsync(startOfMonth);
                var revenueYearTask = _paymentStatisticsRepository.GetRevenueByDateRangeAsync(startOfYear);
                
                var totalTransactionsTask = _paymentStatisticsRepository.GetTotalTransactionsCountAsync(); // Get all transactions (all statuses)
                var completedTransactionsTask = _paymentStatisticsRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Completed);
                var pendingTransactionsTask = _paymentStatisticsRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Pending);
                var failedTransactionsTask = _paymentStatisticsRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Failed);
                
                var transactionsTodayTask = _paymentStatisticsRepository.GetTransactionsCountByDateRangeAsync(today);
                var transactionsWeekTask = _paymentStatisticsRepository.GetTransactionsCountByDateRangeAsync(startOfWeek);
                var transactionsMonthTask = _paymentStatisticsRepository.GetTransactionsCountByDateRangeAsync(startOfMonth);

                await Task.WhenAll(
                    revenueTask, completedRevenueTask, pendingRevenueTask,
                    revenueTodayTask, revenueWeekTask, revenueMonthTask, revenueYearTask,
                    totalTransactionsTask, completedTransactionsTask, pendingTransactionsTask, failedTransactionsTask,
                    transactionsTodayTask, transactionsWeekTask, transactionsMonthTask
                );

                var totalRevenue = await revenueTask;
                var completedTransactions = await completedTransactionsTask;

                var statistics = new RevenueStatisticsDto
                {
                    TotalRevenue = totalRevenue,
                    CompletedRevenue = await completedRevenueTask,
                    PendingRevenue = await pendingRevenueTask,
                    
                    RevenueToday = await revenueTodayTask,
                    RevenueThisWeek = await revenueWeekTask,
                    RevenueThisMonth = await revenueMonthTask,
                    RevenueThisYear = await revenueYearTask,
                    
                    TotalTransactions = await totalTransactionsTask,
                    CompletedTransactions = completedTransactions,
                    PendingTransactions = await pendingTransactionsTask,
                    FailedTransactions = await failedTransactionsTask,
                    
                    AverageTransactionValue = completedTransactions > 0 
                        ? totalRevenue / completedTransactions 
                        : 0,
                    
                    TransactionsToday = await transactionsTodayTask,
                    TransactionsThisWeek = await transactionsWeekTask,
                    TransactionsThisMonth = await transactionsMonthTask
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
                var fromDate = now.AddDays(-days).Date;
                var toDate = now.Date;
                var currentYear = now.Year;

                // Parallel execution để tối ưu performance
                var totalRevenueTask = _paymentStatisticsRepository.GetTotalRevenueAsync();
                var courseRevenueTask = _paymentStatisticsRepository.GetRevenueByProductTypeAsync(ProductType.Course);
                var teacherPackageRevenueTask = _paymentStatisticsRepository.GetRevenueByProductTypeAsync(ProductType.TeacherPackage);
                var dailyRevenueTask = _paymentStatisticsRepository.GetDailyRevenueAsync(fromDate, toDate);
                var monthlyRevenueTask = _paymentStatisticsRepository.GetMonthlyRevenueAsync(currentYear);

                await Task.WhenAll(
                    totalRevenueTask, courseRevenueTask, teacherPackageRevenueTask,
                    dailyRevenueTask, monthlyRevenueTask
                );

                // Get daily revenue breakdown by ProductType in parallel
                var dailyCourseRevenueTask = _paymentStatisticsRepository.GetDailyRevenueByProductTypeAsync(ProductType.Course, fromDate, toDate);
                var dailyTeacherPackageRevenueTask = _paymentStatisticsRepository.GetDailyRevenueByProductTypeAsync(ProductType.TeacherPackage, fromDate, toDate);

                await Task.WhenAll(
                    totalRevenueTask, courseRevenueTask, teacherPackageRevenueTask,
                    dailyRevenueTask, monthlyRevenueTask,
                    dailyCourseRevenueTask, dailyTeacherPackageRevenueTask
                );

                var dailyRevenue = await dailyRevenueTask;
                var monthlyRevenue = await monthlyRevenueTask;
                var dailyCourseRevenue = await dailyCourseRevenueTask;
                var dailyTeacherPackageRevenue = await dailyTeacherPackageRevenueTask;

                // Format data for chart
                var chartData = new RevenueChartDto
                {
                    TotalRevenue = await totalRevenueTask,
                    CourseRevenue = await courseRevenueTask,
                    TeacherPackageRevenue = await teacherPackageRevenueTask,
                    
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
                        .Select(month => new DateTime(currentYear, month, 1))
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
