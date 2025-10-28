using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Service
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
                    response.Message = "Invalid current password";
                    return response;
                }

                // Validate new password strength
                if (!IsPasswordStrong(dto.NewPassword))
                {
                    response.Success = false;
                    response.Message = "Password must contain at least 8 characters, including uppercase, lowercase, number and special character";
                    return response;
                }

                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                await _userRepository.SaveChangesAsync();

                response.Data = true;
                response.Message = "Password changed successfully. Please login again on all devices.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
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
                    response.Message = "Email is required";
                    return response;
                }

                var user = await _userRepository.GetUserByEmailAsync(email);
                
                if (user == null)
                {
                    response.Data = true;
                    response.Message = "If the email exists, an OTP code has been sent";
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

                response.Data = true;
                response.Message = "If the email exists, an OTP code has been sent";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while processing your request: {ex.Message}";
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
                    response.Message = "User not found";
                    return response;
                }

                // Find active OTP token for this user
                var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
                if (otpToken == null)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                if (otpToken.Token != dto.OtpCode)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    response.Success = false;
                    response.Message = "OTP code has expired";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    response.Success = false;
                    response.Message = "OTP code has already been used";
                    return response;
                }

                response.Data = true;
                response.Message = "OTP verified successfully. You can now set your new password.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
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
                    response.Message = "User not found";
                    return response;
                }

                // Find active OTP token for this user
                var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
                if (otpToken == null)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                if (otpToken.Token != dto.OtpCode)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    response.Success = false;
                    response.Message = "OTP code has expired";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    response.Success = false;
                    response.Message = "OTP code has already been used";
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

                response.Data = true;
                response.Message = "Password has been reset successfully";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
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
