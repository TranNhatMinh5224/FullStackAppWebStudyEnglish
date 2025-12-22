using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ILogoutService
    {
        // Đăng xuất
        Task<ServiceResponse<object>> LogoutAsync(LogoutDto dto, int userId);

        // Đăng xuất tất cả thiết bị
        Task<ServiceResponse<object>> LogoutAllAsync(int userId);
    }
}
