using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/statistics")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminStatisticsController : ControllerBase
    {
        private readonly IAdminStatisticsService _adminStatisticsService;

        public AdminStatisticsController(IAdminStatisticsService adminStatisticsService)
        {
            _adminStatisticsService = adminStatisticsService;
        }

        // endpoint Admin lấy tổng quan statistics cho dashboard
        [HttpGet("overview")]
        [RequirePermission("Admin.Revenue.View")]
        public async Task<IActionResult> GetOverviewStatistics()
        {
            var result = await _adminStatisticsService.GetOverviewStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy thống kê users
        [HttpGet("users")]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> GetUserStatistics()
        {
            var result = await _adminStatisticsService.GetUserStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy thống kê doanh thu (revenue)
        [HttpGet("revenue")]
        [RequirePermission("Admin.Revenue.View")]
        public async Task<IActionResult> GetRevenueStatistics()
        {
            var result = await _adminStatisticsService.GetRevenueStatisticsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy dữ liệu doanh thu cho biểu đồ
      
        [HttpGet("revenue/chart")]
        [RequirePermission("Admin.Revenue.View")]
        public async Task<IActionResult> GetRevenueChartData([FromQuery] int days = 30)
        {

            var result = await _adminStatisticsService.GetRevenueChartDataAsync(days);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

