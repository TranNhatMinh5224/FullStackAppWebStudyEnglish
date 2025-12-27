using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Auth;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    // Service xử lý các chức năng liên quan đến mật khẩu
    public class PasswordService : IPasswordService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IEmailService _emailService;

        // Constructor khởi tạo các dependency injection
        public PasswordService(IUserRepository userRepository, IPasswordResetTokenRepository passwordResetTokenRepository, IEmailService emailService)
        {
            _userRepository = userRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _emailService = emailService;
        }

        // Thay đổi mật khẩu cho người dùng đã đăng nhập
        public async Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.VerifyPassword(dto.CurrentPassword))
                {
                    // Ghi log để giám sát bảo mật
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mật khẩu hiện tại không đúng";
                    return response;
                }

                // Kiểm tra độ mạnh của mật khẩu mới
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

                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    response.StatusCode = 200;
                    response.Data = true;
                    response.Message = "Email này chưa được đăng ký trong hệ thống";
                    return response;
                }

                // ANTI-SPAM CHECK 1: Kiểm tra xem user có đang bị block không
                var activeTokens = await _passwordResetTokenRepository.GetActiveTokensByUserIdAsync(user.UserId);
                var blockedToken = activeTokens.FirstOrDefault(t => t.BlockedUntil.HasValue && t.BlockedUntil.Value > DateTime.UtcNow);

                if (blockedToken != null && blockedToken.BlockedUntil.HasValue)
                {
                    var remainingMinutes = Math.Ceiling((blockedToken.BlockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                    response.Success = false;
                    response.StatusCode = 429;
                    response.Message = $"Tài khoản tạm thời bị khóa do nhập sai OTP quá nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút";
                    return response;
                }

                //  Giới hạn gửi OTP (tối đa 3 lần trong 20 phút)
                var recentCount = await _passwordResetTokenRepository.CountRecentTokensByUserIdAsync(user.UserId, 20);
                if (recentCount >= 3)
                {
                    response.Success = false;
                    response.StatusCode = 429;
                    response.Message = "Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau 20 phút";
                    return response;
                }
                // Giới hạn tần suất gửi OTP (cách nhau ít nhất 1 phút)


                var lastToken = activeTokens.OrderByDescending(t => t.CreatedAt).FirstOrDefault();
                if (lastToken != null && (DateTime.UtcNow - lastToken.CreatedAt).TotalMinutes < 1)
                {
                    var remainingSeconds = Math.Ceiling(60 - (DateTime.UtcNow - lastToken.CreatedAt).TotalSeconds);
                    response.Success = false;
                    response.StatusCode = 429;
                    response.Message = $"Vui lòng chờ {remainingSeconds} giây trước khi gửi lại mã OTP";
                    return response;
                }

                // XÓA NGAY các OTP cũ chưa dùng của user này (cleanup trước khi tạo mới)
                // Giúp giữ database sạch sẽ, tránh phình to
                foreach (var oldToken in activeTokens)
                {
                    await _passwordResetTokenRepository.DeleteAsync(oldToken);
                }

                // Cleanup thêm các OTP hết hạn của user này (nếu có)
                await _passwordResetTokenRepository.DeleteExpiredTokensAsync();

                // Generate 6-digit OTP code using OtpHelper
                var otpCode = OtpHelper.GenerateOtpCode();

                var passwordResetToken = new PasswordResetToken
                {
                    Token = otpCode,
                    UserId = user.UserId,
                    ExpiresAt = OtpHelper.GetExpirationTime(5), // 5 minutes
                    AttemptsCount = 0,
                    BlockedUntil = null
                };

                await _passwordResetTokenRepository.AddAsync(passwordResetToken);
                await _passwordResetTokenRepository.SaveChangesAsync();

                // Send OTP email via EmailService
                await _emailService.SendOTPEmailAsync(email, otpCode, user.FirstName);

                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Email khôi phục mật khẩu đã được gửi";
            }
            catch (Exception)
            {
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

                // Check if OTP is expired using OtpHelper
                if (OtpHelper.IsExpired(otpToken.ExpiresAt))
                {
                    // XÓA OTP hết hạn
                    await _passwordResetTokenRepository.DeleteAsync(otpToken);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã hết hạn";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    // XÓA OTP đã sử dụng
                    await _passwordResetTokenRepository.DeleteAsync(otpToken);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã được sử dụng";
                    return response;
                }

                // Validate OTP with brute-force protection using OtpHelper
                var validationResult = OtpHelper.ValidateOtp(dto.OtpCode, otpToken.Token, otpToken.AttemptsCount, maxAttempts: 5);

                if (!validationResult.IsValid)
                {
                    // Handle failed validation
                    if (validationResult.Action == OtpAction.DeleteToken)
                    {
                        // Max attempts reached - delete token
                        await _passwordResetTokenRepository.DeleteAsync(otpToken);
                    }
                    else if (validationResult.Action == OtpAction.UpdateAttempts)
                    {
                        // Update attempts count
                        otpToken.AttemptsCount = validationResult.NewAttemptsCount;
                        await _passwordResetTokenRepository.UpdateAsync(otpToken);
                    }

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = validationResult.Message;
                    return response;
                }

                // ✅ OTP đúng: MARK IsUsed = true (KHÔNG XÓA để SetNewPassword có thể verify lại)
                otpToken.IsUsed = true;
                await _passwordResetTokenRepository.UpdateAsync(otpToken);

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

                // Find OTP token for this user (không filter IsUsed, để lấy token đã verify)
                var otpToken = await _passwordResetTokenRepository.GetByTokenAsync(dto.OtpCode);
                
                if (otpToken == null || otpToken.UserId != user.UserId)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ";
                    return response;
                }

                // Check if OTP is expired using OtpHelper
                if (OtpHelper.IsExpired(otpToken.ExpiresAt))
                {
                    // XÓA OTP hết hạn
                    await _passwordResetTokenRepository.DeleteAsync(otpToken);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã hết hạn";
                    return response;
                }

                // ✅ Check if OTP đã được verify (IsUsed = true)
                if (!otpToken.IsUsed)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Vui lòng xác thực mã OTP trước";
                    return response;
                }

                // ✅ Set password mới
                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                // ✅ XÓA OTP token sau khi set password thành công
                await _passwordResetTokenRepository.DeleteAsync(otpToken);

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

        private static bool IsPasswordStrong(string password)
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
