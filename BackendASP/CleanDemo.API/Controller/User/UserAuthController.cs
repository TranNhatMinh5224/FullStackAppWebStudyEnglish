using Microsoft.AspNetCore.Mvc;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Service.Auth.Register;
using CleanDemo.Application.Service.Auth.Login;
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
        private readonly IAuthenticationService _authenticationService;
        private readonly IUserManagementService _userManagementService;
        private readonly IPasswordService _passwordService;

        public UserAuthController(IRegisterService registerService, ILoginService loginService, IAuthenticationService authenticationService, IUserManagementService userManagementService, IPasswordService passwordService)
        {
            _registerService = registerService;
            _loginService = loginService;
            _authenticationService = authenticationService;
            _userManagementService = userManagementService;
            _passwordService = passwordService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            var result = await _registerService.RegisterUserAsync(dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return CreatedAtAction(nameof(Register), result.Data);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
        {
            var result = await _loginService.LoginUserAsync(dto);
            if (!result.Success) return Unauthorized(new { message = result.Message });
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            // Debug: Check if user is authenticated
            Console.WriteLine($"User authenticated: {User?.Identity?.IsAuthenticated ?? false}");
            Console.WriteLine($"Authentication type: {User?.Identity?.AuthenticationType ?? "None"}");
            
            // Try both Sub and NameIdentifier claim types
            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Sub claim value: {userIdClaim}");
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine("Failed to parse userId from Sub claim");
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _userManagementService.GetUserProfileAsync(userId);
            if (!result.Success) return NotFound(new { message = result.Message });
            return Ok(result.Data);
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
            Console.WriteLine($"Sub claim value: {userIdClaim}");
            
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                Console.WriteLine("Failed to parse userId from Sub claim");
                return Unauthorized(new { message = "Invalid token" });
            }

            Console.WriteLine($"Parsed userId: {userId}");
            var result = await _userManagementService.UpdateUserProfileAsync(userId, dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdClaim = User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value 
                            ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Invalid token" });

            var result = await _passwordService.ChangePasswordAsync(userId, dto);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = "Password changed successfully" });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var result = await _passwordService.ForgotPasswordAsync(dto.Email);
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            Console.WriteLine($"[DEBUG] Controller ResetPassword - Email: {dto?.Email}, OTP: {dto?.OtpCode}, Password Length: {dto?.NewPassword?.Length}");
            
            if (dto == null)
            {
                Console.WriteLine($"[DEBUG] DTO is null");
                return BadRequest(new { message = "Invalid request data" });
            }
            
            var result = await _passwordService.ResetPasswordAsync(dto);
            Console.WriteLine($"[DEBUG] Controller ResetPassword result - Success: {result.Success}, Message: {result.Message}");
            
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        // Add more user-specific endpoints here
    }
}
