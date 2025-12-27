using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Auth
{
    public interface IRegisterService
    {
        // Đăng ký tài khoản
        Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto);

        // Xác thực email
        Task<ServiceResponse<bool>> VerifyEmailAsync(VerifyEmailDto dto);
    }
}
