using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IRegisterService
    {
        // Đăng ký tài khoản
        Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto);

        // Xác thực email
        Task<ServiceResponse<bool>> VerifyEmailAsync(VerifyEmailDto dto);
    }
}
