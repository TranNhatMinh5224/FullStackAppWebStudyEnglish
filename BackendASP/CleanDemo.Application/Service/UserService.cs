using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using CleanDemo.Domain.Entities;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace CleanDemo.Application.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public UserService(IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository, IPasswordResetTokenRepository passwordResetTokenRepository, IMapper mapper, IConfiguration configuration, EmailService emailService)
        {
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordResetTokenRepository = passwordResetTokenRepository;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<ServiceResponse<UserDto>> RegisterUserAsync(RegisterUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var existingUser = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    response.Success = false;
                    response.Message = "Email already exists";
                    return response;
                }

                var user = _mapper.Map<User>(dto);
                user.SetPassword(dto.Password);
                // Assign default role
                user.Roles = new List<Role> { new Role { Name = "User" } };

                await _userRepository.AddUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.Data = _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<AuthResponseDto>> LoginUserAsync(LoginUserDto dto)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null || !user.VerifyPassword(dto.Password))
                {
                    response.Success = false;
                    response.Message = "Invalid email or password";
                    return response;
                }

                var accessToken = GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken(user);

                await _refreshTokenRepository.AddAsync(refreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                response.Data = new AuthResponseDto { AccessToken = accessToken, RefreshToken = refreshToken.Token, User = _mapper.Map<UserDto>(user) };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<UserDto>> GetUserProfileAsync(int userId)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                response.Data = _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<UserDto>> UpdateUserProfileAsync(int userId, UpdateUserDto dto)
        {
            var response = new ServiceResponse<UserDto>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                _mapper.Map(dto, user);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
                await _userRepository.SaveChangesAsync();

                response.Data = _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null || !user.VerifyPassword(dto.CurrentPassword))
                {
                    // Log failed attempt for security monitoring
                    response.Success = false;
                    response.Message = "Invalid current password";
                    return response;
                }

                // Validate new password strength
                if (!IsPasswordStrong(dto.NewPassword))
                {
                    response.Success = false;
                    response.Message = "Password must contain at least 8 characters, including uppercase, lowercase, number and special character";
                    return response;
                }

                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                // Revoke all refresh tokens for security (force re-login on all devices)
                var userTokens = await _refreshTokenRepository.GetTokensByUserIdAsync(userId);
                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(token);
                }

                await _userRepository.SaveChangesAsync();
                await _refreshTokenRepository.SaveChangesAsync();

                response.Data = true;
                response.Message = "Password changed successfully. Please login again on all devices.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private bool IsPasswordStrong(string password)
        {
            if (password.Length < 8) return false;
            
            var hasUpper = password.Any(char.IsUpper);
            var hasLower = password.Any(char.IsLower);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            
            return hasUpper && hasLower && hasNumber && hasSpecial;
        }

        public async Task<ServiceResponse<bool>> ForgotPasswordAsync(string email)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                Console.WriteLine($"[DEBUG] ForgotPassword - Email: {email}");
                
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    Console.WriteLine($"[DEBUG] User not found for email: {email}");
                    // Don't reveal if email exists or not for security
                    response.Data = true;
                    response.Message = "If the email exists, an OTP code has been sent";
                    return response;
                }

                Console.WriteLine($"[DEBUG] User found - UserId: {user.UserId}");

                // TODO: Temporarily disabled - Invalidate any existing active tokens for this user
                // var existingTokens = await _passwordResetTokenRepository.GetActiveTokensByUserIdAsync(user.UserId);
                // foreach (var token in existingTokens)
                // {
                //     token.IsUsed = true;
                //     await _passwordResetTokenRepository.UpdateAsync(token);
                // }
                // Console.WriteLine($"[DEBUG] Invalidated {existingTokens.Count} existing tokens");

                // Generate 6-digit OTP code
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();

                Console.WriteLine($"[DEBUG] Generated OTP: {otpCode}");

                var passwordResetToken = new PasswordResetToken
                {
                    Token = otpCode, // Store OTP code instead of GUID
                    UserId = user.UserId,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(5) // OTP expires in 5 minutes
                };

                Console.WriteLine($"[DEBUG] Saving OTP token - Token: {passwordResetToken.Token}, Expires: {passwordResetToken.ExpiresAt}");

                await _passwordResetTokenRepository.AddAsync(passwordResetToken);
                await _passwordResetTokenRepository.SaveChangesAsync();

                // Send OTP email via EmailService
                await _emailService.SendOTPEmailAsync(email, otpCode, user.SureName);

                Console.WriteLine($"[DEBUG] OTP email sent successfully");

                response.Data = true;
                response.Message = "If the email exists, an OTP code has been sent";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in ForgotPassword: {ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                Console.WriteLine($"[DEBUG] ResetPassword - Email: {dto.Email}, OTP: {dto.OtpCode}");
                
                // Find user by email
                var user = await _userRepository.GetUserByEmailAsync(dto.Email);
                if (user == null)
                {
                    Console.WriteLine($"[DEBUG] User not found for email: {dto.Email}");
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                Console.WriteLine($"[DEBUG] User found - UserId: {user.UserId}");

                // Find active OTP token for this user
                var otpToken = await _passwordResetTokenRepository.GetActiveTokenByUserIdAsync(user.UserId);
                if (otpToken == null)
                {
                    Console.WriteLine($"[DEBUG] No active OTP token found for user: {user.UserId}");
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                Console.WriteLine($"[DEBUG] OTP Token found - Token: {otpToken.Token}, Expires: {otpToken.ExpiresAt}, IsUsed: {otpToken.IsUsed}");

                if (otpToken.Token != dto.OtpCode)
                {
                    Console.WriteLine($"[DEBUG] OTP mismatch - Expected: {otpToken.Token}, Received: {dto.OtpCode}");
                    response.Success = false;
                    response.Message = "Invalid or expired OTP code";
                    return response;
                }

                // Check if OTP is expired (5 minutes)
                if (otpToken.ExpiresAt < DateTime.UtcNow)
                {
                    Console.WriteLine($"[DEBUG] OTP expired - Expires: {otpToken.ExpiresAt}, Now: {DateTime.UtcNow}");
                    response.Success = false;
                    response.Message = "OTP code has expired";
                    return response;
                }

                // Check if OTP is already used
                if (otpToken.IsUsed)
                {
                    Console.WriteLine($"[DEBUG] OTP already used");
                    response.Success = false;
                    response.Message = "OTP code has already been used";
                    return response;
                }

                Console.WriteLine($"[DEBUG] OTP validation passed, updating password...");

                // Update password
                user.SetPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                // Mark OTP token as used
                otpToken.IsUsed = true;
                await _passwordResetTokenRepository.UpdateAsync(otpToken);

                await _userRepository.SaveChangesAsync();
                await _passwordResetTokenRepository.SaveChangesAsync();

                Console.WriteLine($"[DEBUG] Password reset successful");

                response.Data = true;
                response.Message = "Password has been reset successfully";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] Exception in ResetPassword: {ex.Message}");
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<AuthResponseDto>> RefreshTokenAsync(string refreshToken)
        {
            var response = new ServiceResponse<AuthResponseDto>();
            try
            {
                var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow || storedToken.IsRevoked)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired refresh token";
                    return response;
                }

                var user = await _userRepository.GetByIdAsync(storedToken.UserId);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "User not found";
                    return response;
                }

                var newAccessToken = GenerateJwtToken(user);
                var newRefreshToken = GenerateRefreshToken(user);

                // Revoke old token
                storedToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(storedToken);

                // Add new token
                await _refreshTokenRepository.AddAsync(newRefreshToken);
                await _refreshTokenRepository.SaveChangesAsync();

                response.Data = new AuthResponseDto { AccessToken = newAccessToken, RefreshToken = newRefreshToken.Token, User = _mapper.Map<UserDto>(user) };
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> LogoutAsync(string refreshToken)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (storedToken != null)
                {
                    storedToken.IsRevoked = true;
                    await _refreshTokenRepository.UpdateAsync(storedToken);
                    await _refreshTokenRepository.SaveChangesAsync();
                }

                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<ServiceResponse<List<UserDto>>> GetAllUsersAsync()
        {
            var response = new ServiceResponse<List<UserDto>>();
            try
            {
                var users = await _userRepository.GetAllUsersAsync();
                response.Data = _mapper.Map<List<UserDto>>(users);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }
            return response;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "default-key-change-in-production";
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.SureName + " " + user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles to claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8), // 8 hours
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken(User user)
        {
            return new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
