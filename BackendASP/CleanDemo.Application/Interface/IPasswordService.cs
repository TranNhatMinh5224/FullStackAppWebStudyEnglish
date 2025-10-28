using CleanDemo.Application.DTOs;
using CleanDemo.Application.Common;

namespace CleanDemo.Application.Interface
{
    public interface IPasswordService
    {
        Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task<ServiceResponse<bool>> ForgotPasswordAsync(string email);
        Task<ServiceResponse<bool>> VerifyOtpAsync(VerifyOtpDto dto);
        Task<ServiceResponse<bool>> SetNewPasswordAsync(SetNewPasswordDto dto);
    }
}
