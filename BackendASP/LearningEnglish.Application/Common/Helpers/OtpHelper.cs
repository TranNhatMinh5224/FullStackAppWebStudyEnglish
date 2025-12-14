using System.Security.Cryptography;

namespace LearningEnglish.Application.Common.Helpers
{
    /// <summary>
    /// Helper class for OTP (One-Time Password) operations.
    /// Provides cryptographically secure OTP generation and validation logic.
    /// </summary>
    public static class OtpHelper
    {
        /// <summary>
        /// Generates a cryptographically secure 6-digit OTP code.
        /// </summary>
        /// <returns>6-digit OTP string (100000 - 999999)</returns>
        public static string GenerateOtpCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        /// <summary>
        /// Checks if OTP token has expired.
        /// </summary>
        /// <param name="expiresAt">Token expiration time (UTC)</param>
        /// <returns>True if expired, false otherwise</returns>
        public static bool IsExpired(DateTime expiresAt)
        {
            return DateTime.UtcNow > expiresAt;
        }

        /// <summary>
        /// Validates OTP code with brute-force protection.
        /// Returns validation result with updated attempts count and action to take.
        /// </summary>
        /// <param name="inputCode">OTP code from user input</param>
        /// <param name="storedCode">OTP code stored in database</param>
        /// <param name="currentAttempts">Current number of failed attempts</param>
        /// <param name="maxAttempts">Maximum allowed attempts (default: 5)</param>
        /// <returns>OtpValidationResult with success status, remaining attempts, and action</returns>
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

        /// <summary>
        /// Generates OTP expiration time (default: 5 minutes from now in UTC).
        /// </summary>
        /// <param name="minutes">Minutes until expiration (default: 5)</param>
        /// <returns>Expiration DateTime in UTC</returns>
        public static DateTime GetExpirationTime(int minutes = 5)
        {
            return DateTime.UtcNow.AddMinutes(minutes);
        }
    }

    /// <summary>
    /// Result of OTP validation containing validation status and action to take.
    /// </summary>
    public class OtpValidationResult
    {
        /// <summary>
        /// Whether the OTP code is valid (matches stored code).
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Number of remaining attempts before token is deleted.
        /// </summary>
        public int RemainingAttempts { get; set; }

        /// <summary>
        /// Updated attempts count (only set when IsValid = false).
        /// </summary>
        public int NewAttemptsCount { get; set; }

        /// <summary>
        /// Action to take based on validation result.
        /// </summary>
        public OtpAction Action { get; set; }

        /// <summary>
        /// User-friendly message describing the result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Actions to take after OTP validation.
    /// </summary>
    public enum OtpAction
    {
        /// <summary>
        /// OTP is valid - proceed with next step.
        /// </summary>
        Success,

        /// <summary>
        /// OTP is invalid - update attempts count in database.
        /// </summary>
        UpdateAttempts,

        /// <summary>
        /// Max attempts reached - delete token from database.
        /// </summary>
        DeleteToken
    }
}
