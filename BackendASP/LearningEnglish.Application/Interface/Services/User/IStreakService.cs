using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface;

public interface IStreakService
{
    // Lấy streak hiện tại
    Task<ServiceResponse<StreakDto>> GetCurrentStreakAsync(int userId);
    
    // Cập nhật streak
    Task<ServiceResponse<StreakUpdateResultDto>> UpdateStreakAsync(int userId);
    
    // Gửi nhắc nhở streak cho users sắp đứt (LastActivityDate = yesterday)
    Task<ServiceResponse<object>> SendStreakRemindersAsync();
}
