using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface ILoginService
    {
        // Đăng nhập
        Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto);
    }
}
