using System.Runtime.CompilerServices;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.DTOs
{
    // Dto dành cho đăng ký người dùng
    public class RegisterUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
    // Dto dành cho đăng nhập người dùng
    public class LoginUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    // Dto dùng cho thông tin người dùng
    public class UserDto
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? AvatarType { get; set; }
        
        // Streak info
        public StreakDto? Streak { get; set; }
        
        // Teacher subscription info (if user has purchased)
        public UserTeacherSubscriptionDto? TeacherSubscription { get; set; }
    }
    // Dto dành cho phản hồi đăng nhập thành công  
    public class AuthResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserDto User { get; set; } = new();

        public DateTime ExpiresAt { get; set; }
    }
    // Dto dùng cho update user
    public class UpdateUserDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
    
    // Dto riêng cho update avatar
    public class UpdateAvatarDto
    {
        public string AvatarTempKey { get; set; } = string.Empty;
        public string? AvatarType { get; set; }
    }
    // Dto thay đổi mật khẩu
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
    // Dto quên mât khẩu
    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }
    // Dto xác nhận OTP
    public class VerifyOtpDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
    }
    // Dto đặt mật khẩu mới
    public class SetNewPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    // Dto nhận refresh token
    public class ReceiveRefreshTokenDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;

    }
    // Dto phản hồi refresh token
    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
    // Dto phản hồi block account
    public class BlockAccountResponseDto
    {
        public string Message { get; set; } = string.Empty;
    }
    // Dto phản hồi unblock account
    public class UnblockAccountResponseDto
    {
        public string Message { get; set; } = string.Empty;
    }
    // Dto dành cho reponse danh sách user trong all courses
    public class StudentsByAllCoursesDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public int TotalUsers { get; set; }

        public List<UserDto> Users { get; set; } = new();
    }
}
