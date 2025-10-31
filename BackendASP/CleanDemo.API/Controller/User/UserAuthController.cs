using Microsoft.AspNetCore.Mvc;
using CleanDemo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace CleanDemo.API.Controllers.User
{
    [ApiController]
    [Route("api/user/auth")]
    public class UserAuthController : ControllerBase
    {
        private readonly IRegisterService _registerService;
        private readonly ILoginService _loginService;
        private readonly IUserManagementService _userManagementService;
        private readonly IPasswordService _passwordService;

        public UserAuthController(IRegisterService registerService, ILoginService loginService, IUserManagementService userManagementService, IPasswordService passwordService)
        {
            _registerService = registerService;
            _loginService = loginService;
            _userManagementService = userManagementService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _registerService.RegisterUserAsync(dto);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message, data = result.Data });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var result = await _loginService.LoginUserAsync(dto);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message, data = result.Data });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {


            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;


            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine("Failed to parse userId from Sub claim");
                return StatusCode(401, new { success = false, message = "Token không hợp lệ" });
            }

            var result = await _userManagementService.GetUserProfileAsync(userId);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            return StatusCode(result.StatusCode, new { success = result.Success, data = result.Data });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            // Debug: Log all claims
            var allClaims = User?.Claims?.ToList() ?? new List<System.Security.Claims.Claim>();
            Console.WriteLine($"Total claims: {allClaims.Count}");
            foreach (var claim in allClaims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            }

            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine("Failed to parse userId from Sub claim");
                return StatusCode(401, new { success = false, message = "Token không hợp lệ" });
            }

            Console.WriteLine($"Parsed userId: {userId}");
            var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message, data = result.Data });
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return StatusCode(401, new { success = false, message = "Token không hợp lệ" });

            var result = await _passwordService.ChangePasswordAsync(userId, dto);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Email))
            {
                return StatusCode(400, new { success = false, message = "Email là bắt buộc" });
            }

            try
            {
                var result = await _passwordService.ForgotPasswordAsync(dto.Email);
                if (!result.Success)
                    return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi hệ thống" });
            }
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            if (dto == null)
            {
                return StatusCode(400, new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var result = await _passwordService.VerifyOtpAsync(dto);
                if (!result.Success)
                    return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi xác thực OTP" });
            }
        }

        [HttpPost("set-new-password")]
        public async Task<IActionResult> SetNewPassword([FromBody] SetNewPasswordDto dto)
        {
            if (dto == null)
            {
                return StatusCode(400, new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            try
            {
                var result = await _passwordService.SetNewPasswordAsync(dto);
                if (!result.Success)
                    return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, message = "Đã xảy ra lỗi khi đặt lại mật khẩu" });
            }
        }
        // Controller xin cấp refresh token

        // Refresh Token
        [HttpPost("refresh-token")]
        [Authorize(Roles = "User,Admin,Teacher")]
        public async Task<IActionResult> RefreshToken([FromBody] ReceiveRefreshTokenDto request)
        {
            var result = await _loginService.RefreshTokenAsync(request);
            if (!result.Success)
                return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message });
            return StatusCode(result.StatusCode, new { success = result.Success, message = result.Message, data = result.Data });
        }
    }
}
