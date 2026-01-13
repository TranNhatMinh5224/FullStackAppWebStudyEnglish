using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Auth
{
    public interface IPasswordService
    {
        // Đổi mật khẩu
        Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        
        // Quên mật khẩu
        Task<ServiceResponse<bool>> ForgotPasswordAsync(string email);
        
        // Xác thực OTP
        Task<ServiceResponse<bool>> VerifyOtpAsync(VerifyOtpDto dto);
        
        // Đặt mật khẩu mới
        Task<ServiceResponse<bool>> SetNewPasswordAsync(SetNewPasswordDto dto);
    }
}
