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
        private readonly EmailService _emailService;

        public PasswordService(IUserRepository userRepository, IPasswordResetTokenRepository passwordResetTokenRepository, EmailService emailService)
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
                Console.WriteLine($"[DEBUG] ForgotPassword - Email: {email}");

                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    Console.WriteLine($"[DEBUG] User not found for email: {email}");
                    // Don't reveal if email exists or not for security
                    response.Data = true;
                    response.Message = "If the email exists, an OTP code has been sent";
                    return response;
                }

                Console.WriteLine($"[DEBUG] User found - UserId: {user.UserId}");

                // TODO: Temporarily disabled - Invalidate any existing active tokens for this user
                // var existingTokens = await _passwordResetTokenRepository.GetActiveTokensByUserIdAsync(user.UserId);
                // foreach (var token in existingTokens)
                // {
                //     token.IsUsed = true;
                //     await _passwordResetTokenRepository.UpdateAsync(token);
                // }
                // Console.WriteLine($"[DEBUG] Invalidated {existingTokens.Count} existing tokens");

                // Generate 6-digit OTP code
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                Console.WriteLine($"[DEBUG] Generated OTP: {otpCode}");

                var passwordResetToken = new PasswordResetToken
                {
                    Token = otpCode, // Store OTP code instead of GUID
                    UserId = user.UserId,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5) // OTP expires in 5 minutes
                };

                Console.WriteLine($"[DEBUG] Saving OTP token - Token: {passwordResetToken.Token}, Expires: {passwordResetToken.ExpiresAt}");

                await _passwordResetTokenRepository.AddAsync(passwordResetToken);
                await _passwordResetTokenRepository.SaveChangesAsync();

                // Send OTP email via EmailService
                await _emailService.SendOTPEmailAsync(email, otpCode, user.FirstName);

                Console.WriteLine($"[DEBUG] OTP email sent successfully");

                response.Data = true;
                response.Message = "If the email exists, an OTP code has been sent";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in ForgotPassword: {ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                Console.WriteLine($"[DEBUG] ResetPassword - Email: {dto.Email}, OTP: {dto.OtpCode}");

                // Find user by email
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    Console.WriteLine($"[DEBUG] User not found for email: {dto.Email}");
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                Console.WriteLine($"[DEBUG] User found - UserId: {user.UserId}");

                // Find active OTP token for this user
                var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
                if (otpToken == null)
                {
                    Console.WriteLine($"[DEBUG] No active OTP token found for user: {user.UserId}");
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                Console.WriteLine($"[DEBUG] OTP Token found - Token: {otpToken.Token}, Expires: {otpToken.ExpiresAt}, IsUsed: {otpToken.IsUsed}");

                if (otpToken.Token != dto.OtpCode)
                {
                    Console.WriteLine($"[DEBUG] OTP mismatch - Expected: {otpToken.Token}, Received: {dto.OtpCode}");
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    Console.WriteLine($"[DEBUG] OTP expired - Expires: {otpToken.ExpiresAt}, Now: {DateTime.UtcNow}");
                    response.Success = false;
                    response.Message = "OTP code has expired";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    Console.WriteLine($"[DEBUG] OTP already used");
                    response.Success = false;
                    response.Message = "OTP code has already been used";
                    return response;
                }

                Console.WriteLine($"[DEBUG] OTP validation passed, updating password...");

                // Update password
                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                // Mark OTP token as used
                otpToken.IsUsed = true;
                await _passwordResetTokenRepository.UpdateAsync(otpToken);

                await _userRepository.SaveChangesAsync();
                await _passwordResetTokenRepository.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] Password reset successful");

                response.Data = true;
                response.Message = "Password has been reset successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in ResetPassword: {ex.Message}");
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
