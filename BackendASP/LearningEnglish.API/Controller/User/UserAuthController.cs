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
    [Route("api/user/auth")]
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
                throw new UnauthorizedAccessException("Token không hợp lệ");
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

        // PUT: api/user/auth/profile - Update authenticated user's profile information
        [Authorize]
        [HttpPut("update/profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/auth/profile/avatar - Update authenticated user's avatar
        [Authorize]
        [HttpPut("profile/avatar")]
        public async Task<IActionResult> UpdateAvatar([FromBody] UpdateAvatarDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _userManagementService.UpdateAvatarAsync(userId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/auth/change-password - Change password for authenticated user
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            var result = await _passwordService.ChangePasswordAsync(userId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/forgot-password - Request password reset OTP via email
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
            {
                return BadRequest(new { success = false, message = "Email là bắt buộc" });
            }

            var result = await _passwordService.ForgotPasswordAsync(dto.Email);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/verify-otp - Verify OTP code for password reset
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var result = await _passwordService.VerifyOtpAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/set-new-password - Set new password after OTP verification
        [HttpPost("set-new-password")]
        public async Task<IActionResult> SetNewPassword([FromBody] SetNewPasswordDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var result = await _passwordService.SetNewPasswordAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/refresh-token - Refresh expired JWT access token using refresh token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] ReceiveRefreshTokenDto request)
        {
            var result = await _tokenService.RefreshTokenAsync(request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/logout - Logout from current device (revoke specific refresh token)
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutDto dto)
        {
            try 
            {
                var userId = GetCurrentUserId();
                var result = await _logoutService.LogoutAsync(dto, userId);
                return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Đã xảy ra lỗi hệ thống khi đăng xuất",
                    error = ex.Message 
                });
            }
        }

        // POST: api/user/auth/logout-all - Logout from all devices (revoke all refresh tokens)
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutAll()
        {
            try 
            {
                var userId = GetCurrentUserId();
                var result = await _logoutService.LogoutAllAsync(userId);
                return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Đã xảy ra lỗi hệ thống khi đăng xuất khỏi tất cả thiết bị",
                    error = ex.Message 
                });
            }
        }
    }
}
