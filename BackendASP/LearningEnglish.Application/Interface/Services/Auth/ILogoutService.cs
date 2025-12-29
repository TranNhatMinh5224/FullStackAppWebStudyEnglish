using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface.Auth
{
    public interface ILogoutService
    {
        // Đăng xuất
        Task<ServiceResponse<object>> LogoutAsync(LogoutDto dto, int userId);

        // Đăng xuất tất cả thiết bị
        Task<ServiceResponse<object>> LogoutAllAsync(int userId);
    }
}
