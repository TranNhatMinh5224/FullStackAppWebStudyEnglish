using System.Security.Cryptography;

namespace LearningEnglish.Application.Common.Helpers
{
    // Helper class cho các thao tác OTP (One-Time Password)
    // Cung cấp logic tạo và validate OTP an toàn về mặt mã hóa
    public static class OtpHelper
    {
        // Tạo mã OTP 6 chữ số an toàn (100000 - 999999)
        public static string GenerateOtpCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        // Kiểm tra xem OTP token đã hết hạn chưa
        public static bool IsExpired(DateTime expiresAt)
        {
            return DateTime.UtcNow > expiresAt;
        }

        // Validate mã OTP với bảo vệ chng brute-force
        // Trả về kết quả validation với số lần thử còn lại và hành động cần thực hiện
        public static OtpValidationResult ValidateOtp(
            string inputCode, 
            string storedCode, 
            int currentAttempts, 
            int maxAttempts = 5)
        {
            // Check if OTP matches
            if (inputCode == storedCode)
            {
                return new OtpValidationResult
                {
                    IsValid = true,
                    RemainingAttempts = maxAttempts - currentAttempts,
                    Action = OtpAction.Success
                };
            }

            // OTP mismatch - increment attempts
            var newAttempts = currentAttempts + 1;

            // Check if max attempts reached
            if (newAttempts >= maxAttempts)
            {
                return new OtpValidationResult
                {
                    IsValid = false,
                    RemainingAttempts = 0,
                    NewAttemptsCount = newAttempts,
                    Action = OtpAction.DeleteToken,
                    Message = $"Bạn đã nhập sai OTP quá {maxAttempts} lần. Vui lòng yêu cầu mã OTP mới"
                };
            }

            // Still have attempts left
            return new OtpValidationResult
            {
                IsValid = false,
                RemainingAttempts = maxAttempts - newAttempts,
                NewAttemptsCount = newAttempts,
                Action = OtpAction.UpdateAttempts,
                Message = $"Mã OTP không chính xác. Còn {maxAttempts - newAttempts} lần thử"
            };
        }

        // Generates OTP expiration time (default: 5 minutes from now in UTC)
        // minutes: Minutes until expiration (default: 5)
        // returns: Expiration DateTime in UTC
        public static DateTime GetExpirationTime(int minutes = 5)
        {
            return DateTime.UtcNow.AddMinutes(minutes);
        }
    }

    // Result of OTP validation containing validation status and action to take
    // Kết quả validation OTP
    public class OtpValidationResult
    {
        // Mã OTP có hợp lệ không (khớp với mã đã lưu)
        public bool IsValid { get; set; }

        // Số lần thử còn lại trước khi token bị xóa
        public int RemainingAttempts { get; set; }

        // Số lần thử mới (chỉ set khi IsValid = false)
        public int NewAttemptsCount { get; set; }

        // Hành động cần thực hiện dựa trên kết quả validation
        public OtpAction Action { get; set; }

        // Thông báo mô tả kết quả
        public string Message { get; set; } = string.Empty;
    }

    // Các hành động sau khi validate OTP
    public enum OtpAction
    {
        // OTP hợp lệ - tiếp tục bước tiếp theo
        Success,

        // OTP không hợp lệ - cập nhật số lần thử trong database
        UpdateAttempts,

        // Đã thử tối đa - xóa token khỏi database
        DeleteToken
    }
}
