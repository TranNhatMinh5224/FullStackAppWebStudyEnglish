using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface;

public interface IStreakService
{
    Task<ServiceResponse<StreakDto>> GetCurrentStreakAsync(int userId);
    Task<ServiceResponse<StreakUpdateResultDto>> UpdateStreakAsync(int userId, bool isSuccessful);
    Task<ServiceResponse<int>> GetLongestStreakAsync(int userId);
    Task<ServiceResponse<List<StreakHistoryDto>>> GetStreakHistoryAsync(int userId, int days = 30);
    Task<ServiceResponse<StreakDto>> ResetStreakAsync(int userId);
}
