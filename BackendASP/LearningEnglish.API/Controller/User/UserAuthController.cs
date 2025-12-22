using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LearningEnglish.API.Controllers.User
{
    // Controller xử lý các chức năng xác thực người dùng
    [ApiController]
    [Route("api/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly IUserManagementService _userManagementService;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;
        private readonly IGoogleLoginService _googleLoginService;
        private readonly IFacebookLoginService _facebookLoginService;
        private readonly ILogoutService _logoutService;

        // Constructor khởi tạo các dependency injection
        public UserAuthController(
            IRegisterService registerService,
            ILoginService loginService,
            IUserManagementService userManagementService,
            IPasswordService passwordService,
            ITokenService tokenService,
            IGoogleLoginService googleLoginService,
            IFacebookLoginService facebookLoginService,
            ILogoutService logoutService)
        {
            _registerService = registerService;
            _loginService = loginService;
            _userManagementService = userManagementService;
            _passwordService = passwordService;
            _tokenService = tokenService;
            _googleLoginService = googleLoginService;
            _facebookLoginService = facebookLoginService;
            _logoutService = logoutService;
        }

        // Lấy ID người dùng hiện tại từ JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return 0;
            }

            return userId;
        }

        // Đăng ký tài khoản người dùng mới
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _registerService.RegisterUserAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/verify-email - Verify email with OTP code
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        {
            var result = await _registerService.VerifyEmailAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // Đăng nhập bằng email và mật khẩu
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var result = await _loginService.LoginUserAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // Đăng nhập bằng Google OAuth
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var result = await _googleLoginService.HandleGoogleLoginAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // Đăng nhập bằng Facebook OAuth
        [HttpPost("facebook-login")]
        public async Task<IActionResult> FacebookLogin([FromBody] FacebookLoginDto dto)
        {
            var result = await _facebookLoginService.HandleFacebookLoginAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // Lấy thông tin profile người dùng đã đăng nhập
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            var result = await _userManagementService.GetUserProfileAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/auth/profile - Cập nhật thông tin profile người dùng đã đăng nhập
        [Authorize]
        [HttpPut("update/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/auth/profile/avatar - sửa đổi avatar người dùng đã đăng nhập
        [Authorize]
        [HttpPut("profile/avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _userManagementService.UpdateAvatarAsync(userId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/auth/change-password - thay đổi mật khẩu người dùng đã đăng nhập
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _passwordService.ChangePasswordAsync(userId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/forgot-password - yêu cầu đặt lại mật khẩu
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _passwordService.ForgotPasswordAsync(dto.Email);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/verify-otp - đặt lại mật khẩu bằng mã OTP
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            var result = await _passwordService.VerifyOtpAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/set-new-password - đặt mật khẩu mới sau khi xác minh OTP
        [HttpPost("set-new-password")]
        public async Task<IActionResult> SetNewPassword([FromBody] SetNewPasswordDto dto)
        {
            var result = await _passwordService.SetNewPasswordAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/refresh-token - làm mới access token bằng refresh token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] ReceiveRefreshTokenDto request)
        {
            var result = await _tokenService.RefreshTokenAsync(request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/logout - đăng xuất khỏi thiết bị hiện tại
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _logoutService.LogoutAsync(dto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/logout-all - đăng xuất khỏi tất cả các thiết bị
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            var userId = GetCurrentUserId();
            var result = await _logoutService.LogoutAllAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
