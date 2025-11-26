using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface IProgressDashboardService
    {
        Task<ServiceResponse<UserProgressDashboardDto>> GetUserProgressDashboardAsync(int userId);
        Task<ServiceResponse<ProgressStatisticsDto>> GetProgressStatisticsAsync(int userId);
    }
}