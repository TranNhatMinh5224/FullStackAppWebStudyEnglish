using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ILogoutService
    {
        // Đăng xuất khỏi device hiện tại
        Task<ServiceResponse<object>> LogoutAsync(LogoutDto dto, int userId);

        // Đăng xuất khỏi tất cả devices
        Task<ServiceResponse<object>> LogoutAllAsync(int userId);
    }
}
