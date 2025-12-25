using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminStatisticsService : IAdminStatisticsService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly ILogger<AdminStatisticsService> _logger;

        public AdminStatisticsService(
            IUserRepository userRepository,
            ICourseRepository courseRepository,
            IPaymentRepository paymentRepository,
            ILogger<AdminStatisticsService> logger)
        {
            _userRepository = userRepository;
            _courseRepository = courseRepository;
            _paymentRepository = paymentRepository;
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

                // Parallel execution để tối ưu performance
                var tasks = new[]
                {
                    _userRepository.GetTotalUsersCountAsync(),
                    _userRepository.GetUserCountByRoleAsync("Student"),
                    _userRepository.GetUserCountByRoleAsync("Teacher"),
                    _userRepository.GetUserCountByRoleAsync("Admin"),
                    _courseRepository.GetTotalCoursesCountAsync(),
                    _courseRepository.GetCourseCountByTypeAsync(CourseType.System),
                    _courseRepository.GetCourseCountByTypeAsync(CourseType.Teacher),
                    _courseRepository.GetTotalEnrollmentsCountAsync(),
                    _userRepository.GetNewUsersCountAsync(thirtyDaysAgo),
                    _courseRepository.GetNewCoursesCountAsync(thirtyDaysAgo)
                };

                var results = await Task.WhenAll(tasks);

                var statistics = new AdminOverviewStatisticsDto
                {
                    TotalUsers = results[0],
                    TotalStudents = results[1],
                    TotalTeachers = results[2],
                    TotalAdmins = results[3],
                    TotalCourses = results[4],
                    TotalSystemCourses = results[5],
                    TotalTeacherCourses = results[6],
                    TotalEnrollments = results[7],
                    NewUsersLast30Days = results[8],
                    NewCoursesLast30Days = results[9],
                    TotalRevenue = 0 // TODO: Implement payment repository khi có
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
                    _userRepository.GetTotalUsersCountAsync(),
                    _userRepository.GetUserCountByRoleAsync("Student"),
                    _userRepository.GetUserCountByRoleAsync("Teacher"),
                    _userRepository.GetUserCountByRoleAsync("Admin"),
                    _userRepository.GetActiveUsersCountAsync(),
                    _userRepository.GetBlockedUsersCountAsync(),
                    _userRepository.GetNewUsersCountAsync(today),
                    _userRepository.GetNewUsersCountAsync(weekAgo),
                    _userRepository.GetNewUsersCountAsync(monthAgo)
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

        // Lấy chi tiết thống kê courses
        public async Task<ServiceResponse<CourseStatisticsDto>> GetCourseStatisticsAsync()
        {
            var response = new ServiceResponse<CourseStatisticsDto>();

            try
            {
                _logger.LogInformation("Fetching course statistics");

                var monthAgo = DateTime.UtcNow.AddMonths(-1);

                // Parallel execution
                var tasks = new[]
                {
                    _courseRepository.GetTotalCoursesCountAsync(),
                    _courseRepository.GetCourseCountByTypeAsync(CourseType.System),
                    _courseRepository.GetCourseCountByTypeAsync(CourseType.Teacher),
                    _courseRepository.GetCourseCountByStatusAsync(CourseStatus.Published),
                    _courseRepository.GetCourseCountByStatusAsync(CourseStatus.Draft),
                    _courseRepository.GetNewCoursesCountAsync(monthAgo),
                    _courseRepository.GetTotalEnrollmentsCountAsync()
                };

                var results = await Task.WhenAll(tasks);

                var totalCourses = results[0];
                var totalEnrollments = results[6];

                var statistics = new CourseStatisticsDto
                {
                    TotalCourses = totalCourses,
                    SystemCourses = results[1],
                    TeacherCourses = results[2],
                    PublishedCourses = results[3],
                    DraftCourses = results[4],
                    NewCoursesThisMonth = results[5],
                    TotalEnrollments = totalEnrollments,
                    AverageEnrollmentsPerCourse = totalCourses > 0 
                        ? (decimal)totalEnrollments / totalCourses 
                        : 0
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê courses thành công";

                _logger.LogInformation("Successfully fetched course statistics");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thống kê courses";
                _logger.LogError(ex, "Error fetching course statistics");
            }

            return response;
        }

        // Lấy chi tiết thống kê teachers
        public async Task<ServiceResponse<TeacherStatisticsDto>> GetTeacherStatisticsAsync()
        {
            var response = new ServiceResponse<TeacherStatisticsDto>();

            try
            {
                _logger.LogInformation("Fetching teacher statistics");

                var today = DateTime.UtcNow.Date;
                var weekAgo = DateTime.UtcNow.AddDays(-7);
                var monthAgo = DateTime.UtcNow.AddMonths(-1);

                // Parallel execution
                var tasks = new[]
                {
                    _userRepository.GetUserCountByRoleAsync("Teacher"),
                    _userRepository.GetActiveUsersCountAsync(),
                    _userRepository.GetBlockedUsersCountAsync(),
                    _userRepository.GetNewUsersCountAsync(today),
                    _userRepository.GetNewUsersCountAsync(weekAgo),
                    _userRepository.GetNewUsersCountAsync(monthAgo),
                    _courseRepository.GetCoursesCountByTeachersAsync(),
                    _courseRepository.GetPublishedCoursesCountByTeachersAsync(),
                    _courseRepository.GetEnrollmentsCountForTeacherCoursesAsync()
                };

                var results = await Task.WhenAll(tasks);

                var totalTeachers = results[0];
                var totalCoursesCreated = results[6];

                var statistics = new TeacherStatisticsDto
                {
                    TotalTeachers = totalTeachers,
                    ActiveTeachers = results[1],
                    BlockedTeachers = results[2],
                    NewTeachersToday = results[3],
                    NewTeachersThisWeek = results[4],
                    NewTeachersThisMonth = results[5],
                    TotalCoursesCreated = totalCoursesCreated,
                    PublishedCoursesCreated = results[7],
                    AverageCoursesPerTeacher = totalTeachers > 0 
                        ? (decimal)totalCoursesCreated / totalTeachers 
                        : 0,
                    TotalEnrollmentsForTeacherCourses = results[8]
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê teachers thành công";

                _logger.LogInformation("Successfully fetched teacher statistics");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thống kê teachers";
                _logger.LogError(ex, "Error fetching teacher statistics");
            }

            return response;
        }

        // Lấy chi tiết thống kê students
        public async Task<ServiceResponse<StudentStatisticsDto>> GetStudentStatisticsAsync()
        {
            var response = new ServiceResponse<StudentStatisticsDto>();

            try
            {
                _logger.LogInformation("Fetching student statistics");

                var today = DateTime.UtcNow.Date;
                var weekAgo = DateTime.UtcNow.AddDays(-7);
                var monthAgo = DateTime.UtcNow.AddMonths(-1);

                // Parallel execution
                var tasks = new[]
                {
                    _userRepository.GetUserCountByRoleAsync("Student"),
                    _userRepository.GetActiveUsersCountAsync(),
                    _userRepository.GetBlockedUsersCountAsync(),
                    _userRepository.GetNewUsersCountAsync(today),
                    _userRepository.GetNewUsersCountAsync(weekAgo),
                    _userRepository.GetNewUsersCountAsync(monthAgo),
                    _courseRepository.GetTotalEnrollmentsCountAsync(),
                    _courseRepository.GetStudentsWithEnrollmentsCountAsync(),
                    _courseRepository.GetActiveStudentsInCoursesCountAsync()
                };

                var results = await Task.WhenAll(tasks);

                var totalStudents = results[0];
                var totalEnrollments = results[6];
                var studentsWithEnrollments = results[7];

                var statistics = new StudentStatisticsDto
                {
                    TotalStudents = totalStudents,
                    ActiveStudents = results[1],
                    BlockedStudents = results[2],
                    NewStudentsToday = results[3],
                    NewStudentsThisWeek = results[4],
                    NewStudentsThisMonth = results[5],
                    TotalEnrollments = totalEnrollments,
                    StudentsWithEnrollments = studentsWithEnrollments,
                    AverageEnrollmentsPerStudent = totalStudents > 0 
                        ? (decimal)totalEnrollments / totalStudents 
                        : 0,
                    ActiveStudentsInCourses = results[8]
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê students thành công";

                _logger.LogInformation("Successfully fetched student statistics");
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy thống kê students";
                _logger.LogError(ex, "Error fetching student statistics");
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
                var revenueTask = _paymentRepository.GetTotalRevenueAsync();
                var completedRevenueTask = _paymentRepository.GetRevenueByStatusAsync(PaymentStatus.Completed);
                var pendingRevenueTask = _paymentRepository.GetRevenueByStatusAsync(PaymentStatus.Pending);
                
                var revenueTodayTask = _paymentRepository.GetRevenueByDateRangeAsync(today);
                var revenueWeekTask = _paymentRepository.GetRevenueByDateRangeAsync(startOfWeek);
                var revenueMonthTask = _paymentRepository.GetRevenueByDateRangeAsync(startOfMonth);
                var revenueYearTask = _paymentRepository.GetRevenueByDateRangeAsync(startOfYear);
                
                var totalTransactionsTask = _paymentRepository.GetTransactionCountAsync(0); // Get all transactions
                var completedTransactionsTask = _paymentRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Completed);
                var pendingTransactionsTask = _paymentRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Pending);
                var failedTransactionsTask = _paymentRepository.GetTransactionsCountByStatusAsync(PaymentStatus.Failed);
                
                var transactionsTodayTask = _paymentRepository.GetTransactionsCountByDateRangeAsync(today);
                var transactionsWeekTask = _paymentRepository.GetTransactionsCountByDateRangeAsync(startOfWeek);
                var transactionsMonthTask = _paymentRepository.GetTransactionsCountByDateRangeAsync(startOfMonth);

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
                var totalRevenueTask = _paymentRepository.GetTotalRevenueAsync();
                var courseRevenueTask = _paymentRepository.GetRevenueByProductTypeAsync(ProductType.Course);
                var teacherPackageRevenueTask = _paymentRepository.GetRevenueByProductTypeAsync(ProductType.TeacherPackage);
                var dailyRevenueTask = _paymentRepository.GetDailyRevenueAsync(fromDate, toDate);
                var monthlyRevenueTask = _paymentRepository.GetMonthlyRevenueAsync(currentYear);

                await Task.WhenAll(
                    totalRevenueTask, courseRevenueTask, teacherPackageRevenueTask,
                    dailyRevenueTask, monthlyRevenueTask
                );

                var dailyRevenue = await dailyRevenueTask;
                var monthlyRevenue = await monthlyRevenueTask;

                // Query daily revenue by ProductType
                var dailyCourseRevenueDict = new Dictionary<DateTime, decimal>();
                var dailyTeacherPackageRevenueDict = new Dictionary<DateTime, decimal>();

                // Get daily breakdown by ProductType in parallel
                var courseDailyTask = Task.Run(async () =>
                {
                    var payments = await _paymentRepository.GetRevenueByProductTypeAndDateRangeAsync(
                        ProductType.Course, fromDate, toDate);
                    return await _paymentRepository.GetDailyRevenueAsync(fromDate, toDate);
                });

                var teacherPackageDailyTask = Task.Run(async () =>
                {
                    var payments = await _paymentRepository.GetRevenueByProductTypeAndDateRangeAsync(
                        ProductType.TeacherPackage, fromDate, toDate);
                    return await _paymentRepository.GetDailyRevenueAsync(fromDate, toDate);
                });

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
