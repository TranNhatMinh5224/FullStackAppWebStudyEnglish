using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminStatisticsService
    {
        // Lấy tổng quan statistics cho dashboard
        Task<ServiceResponse<AdminOverviewStatisticsDto>> GetOverviewStatisticsAsync();
        
        // Lấy chi tiết thống kê users
        Task<ServiceResponse<UserStatisticsDto>> GetUserStatisticsAsync();
        
        // Lấy chi tiết thống kê doanh thu
        Task<ServiceResponse<RevenueStatisticsDto>> GetRevenueStatisticsAsync();
        
        // Lấy dữ liệu doanh thu cho biểu đồ (simplified)
        Task<ServiceResponse<RevenueChartDto>> GetRevenueChartDataAsync(int days = 30);
    }
}
