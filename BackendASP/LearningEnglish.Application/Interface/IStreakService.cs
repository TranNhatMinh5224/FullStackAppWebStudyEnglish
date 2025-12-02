using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface;

public interface IStreakService
{
    Task<ServiceResponse<StreakDto>> GetCurrentStreakAsync(int userId);
    Task<ServiceResponse<StreakUpdateResultDto>> UpdateStreakAsync(int userId, bool isSuccessful);
}
