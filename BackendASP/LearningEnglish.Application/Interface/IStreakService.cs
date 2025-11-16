using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface;

public interface IStreakService
{
    Task<ServiceResponse<int>> GetCurrentStreakAsync(int userId);
    Task<ServiceResponse<bool>> UpdateStreakAsync(int userId, bool isSuccessful);
    Task<ServiceResponse<int>> GetLongestStreakAsync(int userId);
}
