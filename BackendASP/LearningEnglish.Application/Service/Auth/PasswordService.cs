using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
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

                // Generate 6-digit OTP code
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                var passwordResetToken = new PasswordResetToken
                {
                    Token = otpCode,
                    UserId = user.UserId,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5), // OTP expires in 5 minutes
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

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    // XÓA OTP hết hạn - không còn khả năng sử dụng
                    await _passwordResetTokenRepository.DeleteAsync(otpToken);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP không hợp lệ hoặc đã hết hạn";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    // XÓA OTP đã sử dụng - không còn khả năng sử dụng
                    await _passwordResetTokenRepository.DeleteAsync(otpToken);

                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Mã OTP đã được sử dụng";
                    return response;
                }

                // ANTI-SPAM CHECK: Kiểm tra xem token có đang bị block không
                if (otpToken.BlockedUntil.HasValue && otpToken.BlockedUntil.Value > DateTime.UtcNow)
                {
                    // XÓA OTP bị khóa - không còn khả năng sử dụng
                    await _passwordResetTokenRepository.DeleteAsync(otpToken);

                    var remainingMinutes = Math.Ceiling((otpToken.BlockedUntil.Value - DateTime.UtcNow).TotalMinutes);
                    response.Success = false;
                    response.StatusCode = 429;
                    response.Message = $"Tài khoản tạm khóa đến {otpToken.BlockedUntil.Value.AddHours(7):HH:mm dd/MM/yyyy}. Vui lòng thử lại sau {remainingMinutes} phút";
                    return response;
                }

                // Verify OTP code
                if (otpToken.Token != dto.OtpCode)
                {
                    // BRUTE-FORCE PROTECTION: Tăng số lần thử sai
                    otpToken.AttemptsCount++;

                    // Nếu nhập sai >= 5 lần, khóa 20 phút
                    if (otpToken.AttemptsCount >= 5)
                    {
                        // XÓA OTP bị khóa - không còn khả năng sử dụng
                        await _passwordResetTokenRepository.DeleteAsync(otpToken);

                        response.Success = false;
                        response.StatusCode = 429;
                        response.Message = "Bạn đã nhập sai OTP quá 5 lần. Tài khoản bị khóa trong 20 phút";
                        return response;
                    }

                    // Nếu nhập sai >= 10 lần, xóa token và bắt gửi lại OTP mới
                    if (otpToken.AttemptsCount >= 10)
                    {
                        // XÓA OTP quá nhiều lần thử - không còn khả năng sử dụng
                        await _passwordResetTokenRepository.DeleteAsync(otpToken);

                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Quá nhiều lần thử. Vui lòng yêu cầu mã OTP mới";
                        return response;
                    }

                    await _passwordResetTokenRepository.UpdateAsync(otpToken);

                    var remainingAttempts = 5 - otpToken.AttemptsCount;
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Mã OTP không chính xác. Còn {remainingAttempts} lần thử";
                    return response;
                }

                // OTP đúng: Xóa OTP khỏi database (không cần lưu lại)
                await _passwordResetTokenRepository.DeleteAsync(otpToken);

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
