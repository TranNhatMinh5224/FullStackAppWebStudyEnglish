using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/admin/statistics")]
    [Authorize(Roles = "Admin")]
    public class AdminStatisticsController : ControllerBase
    {
        private readonly IAdminStatisticsService _statisticsService;
        private readonly ILogger<AdminStatisticsController> _logger;

        public AdminStatisticsController(
            IAdminStatisticsService statisticsService,
            ILogger<AdminStatisticsController> logger)
        {
            _statisticsService = statisticsService;
            _logger = logger;
        }

        // GET: api/admin/statistics/overview - Lấy tổng quan thống kê cho dashboard
        // Trả về: Tổng users, courses, enrollments, revenue, new users/courses 30 ngày
        [HttpGet("overview")]
        public async Task<IActionResult> GetOverviewStatistics()
        {
            _logger.LogInformation("Admin đang lấy tổng quan thống kê");
            
            var result = await _statisticsService.GetOverviewStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/statistics/users - Lấy thống kê chi tiết về users
        // Trả về: Tổng users theo role, active/blocked, new users theo ngày/tuần/tháng
        [HttpGet("users")]
        public async Task<IActionResult> GetUserStatistics()
        {
            _logger.LogInformation("Admin đang lấy thống kê users");
            
            var result = await _statisticsService.GetUserStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/statistics/courses - Lấy thống kê chi tiết về courses
        // Trả về: Tổng courses theo type/status, enrollments, average enrollments
        [HttpGet("courses")]
        public async Task<IActionResult> GetCourseStatistics()
        {
            _logger.LogInformation("Admin đang lấy thống kê courses");
            
            var result = await _statisticsService.GetCourseStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/statistics/teachers - Lấy thống kê chi tiết về teachers
        // Trả về: Tổng teachers, active/blocked, courses created, average courses, enrollments
        [HttpGet("teachers")]
        public async Task<IActionResult> GetTeacherStatistics()
        {
            _logger.LogInformation("Admin đang lấy thống kê teachers");
            
            var result = await _statisticsService.GetTeacherStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/statistics/students - Lấy thống kê chi tiết về students
        // Trả về: Tổng students, active/blocked, enrollments, average enrollments, active students
        [HttpGet("students")]
        public async Task<IActionResult> GetStudentStatistics()
        {
            _logger.LogInformation("Admin đang lấy thống kê students");
            
            var result = await _statisticsService.GetStudentStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/statistics/revenue - Lấy thống kê chi tiết về doanh thu
        // Trả về: Tổng doanh thu, doanh thu theo status, theo thời gian, giao dịch, trung bình
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueStatistics()
        {
            _logger.LogInformation("Admin đang lấy thống kê doanh thu");
            
            var result = await _statisticsService.GetRevenueStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/statistics/revenue/chart - Lấy dữ liệu doanh thu cho biểu đồ
        // Query params: days (default 30) - số ngày gần đây để hiển thị
        // Trả về: Tổng doanh thu, breakdown theo Course/TeacherPackage, timeline data
        [HttpGet("revenue/chart")]
        public async Task<IActionResult> GetRevenueChartData([FromQuery] int days = 30)
        {
            _logger.LogInformation("Admin đang lấy dữ liệu biểu đồ doanh thu (last {Days} days)", days);
            
            if (days < 1 || days > 365)
            {
                return BadRequest(new { message = "Days must be between 1 and 365" });
            }
            
            var result = await _statisticsService.GetRevenueChartDataAsync(days);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
