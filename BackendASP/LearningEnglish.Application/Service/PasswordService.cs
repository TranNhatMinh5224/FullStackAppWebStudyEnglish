using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class PasswordService : IPasswordService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailService _emailService;

        public PasswordService(IUserRepository userRepository, IPasswordResetTokenRepository passwordResetTokenRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
        }

        public async Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.VerifyPassword(dto.CurrentPassword))
                {
                    // Log failed attempt for security monitoring
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mật khẩu hiện tại không đúng";
                    return response;
                }

                // Validate new password strength
                if (!IsPasswordStrong(dto.NewPassword))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt";
                    return response;
                }

                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                await _userRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đổi mật khẩu thành công";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ForgotPasswordAsync(string email)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Validate email input
                if (string.IsNullOrEmpty(email))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Email là bắt buộc";
                    return response;
                }
                //

                var user = await _userRepository.GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    response.StatusCode = 200;
                    response.Data = true;
                    response.Message = "Email này chưa được đăng ký trong hệ thống";
                    return response;
                }

                // Generate 6-digit OTP code
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                var passwordResetToken = new PasswordResetToken
                {
                    Token = otpCode, 
                    UserId = user.UserId, 
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5) // OTP expires in 5 minutes
                };

                await _passwordResetTokenRepository.AddAsync(passwordResetToken);
                await _passwordResetTokenRepository.SaveChangesAsync();

                // Send OTP email via EmailService
                await _emailService.SendOTPEmailAsync(email, otpCode, user.FirstName);

                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Email khôi phục mật khẩu đã được gửi";
            }
            catch (Exception ex)
            {
                // Log chi tiết exception để debug
                Console.WriteLine($"[ERROR] ForgotPasswordAsync Exception: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[ERROR] InnerException: {ex.InnerException.Message}");
                }
                
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Find user by email
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                // Find active OTP token for this user
                var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
                if (otpToken == null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                if (otpToken.Token != dto.OtpCode)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã được sử dụng";
                    return response;
                }

                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Xác thực mã OTP thành công";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> SetNewPasswordAsync(SetNewPasswordDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Find user by email
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy người dùng";
                    return response;
                }

                // Find active OTP token for this user
                var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
                if (otpToken == null)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                if (otpToken.Token != dto.OtpCode)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã được sử dụng";
                    return response;
                }

                // Update password
                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                // Mark OTP token as used
                otpToken.IsUsed = true;
                await _passwordResetTokenRepository.UpdateAsync(otpToken);

                await _userRepository.SaveChangesAsync();
                await _passwordResetTokenRepository.SaveChangesAsync();

                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đặt lại mật khẩu thành công";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
            }
            return response;
        }

        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8) return false;

            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasNumber && hasSpecial;
        }
    }
}
