using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LearningEnglish.API.Controllers.User
{
    [ApiController]
    [Route("api/user/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly IUserManagementService _userManagementService;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public UserAuthController(
            IRegisterService registerService,
            ILoginService loginService,
            IUserManagementService userManagementService,
            IPasswordService passwordService,
            ITokenService tokenService)
        {
            _registerService = registerService;
            _loginService = loginService;
            _userManagementService = userManagementService;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

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

        // POST: api/user/auth/register - Register new user account
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _registerService.RegisterUserAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/auth/login - Authenticate user and return JWT tokens
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var result = await _loginService.LoginUserAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/auth/profile - Get authenticated user's profile information
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
        [HttpPut("profile")]
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
    }
}
