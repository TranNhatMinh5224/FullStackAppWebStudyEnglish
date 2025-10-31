using CleanDemo.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
namespace CleanDemo.Application.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(IConfiguration configuration, IUserRepository userRepository, IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public Tuple<string, DateTime> GenerateAccessToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "default-issuer";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "default-audience";


            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Add roles to claims
            foreach (var role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds);

            var tokenjwt = new JwtSecurityTokenHandler().WriteToken(token);
            return Tuple.Create(tokenjwt, expiresAt);
        }

        public RefreshToken GenerateRefreshToken(User user)
        {
            return new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                UserId = user.UserId,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
        }
        // Lấy ClaimsPrincipal từ access token đã hết hạn

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
            var jwtAudience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,
                ValidateAudience = true,
                ValidAudience = jwtAudience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = false
            };
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Token không hợp lệ");

            return principal;
        }

        public async Task<ServiceResponse<RefreshTokenResponseDto>> RefreshTokenAsync(ReceiveRefreshTokenDto request)
        {
            var response = new ServiceResponse<RefreshTokenResponseDto>();
            try
            {
                var refreshToken = request.RefreshToken;
                var accessToken = request.AccessToken;
                
                var existingToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
                if (existingToken == null || existingToken.ExpiresAt < DateTime.UtcNow)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Refresh token không hợp lệ hoặc đã hết hạn.";
                    return response;
                }
                
                if (existingToken.IsRevoked)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Refresh token đã bị thu hồi.";
                    return response;
                }
                
                // Giải mã access token  
                var tokenprincipal = GetPrincipalFromExpiredToken(accessToken);
                if (tokenprincipal == null)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Access token không hợp lệ.";
                    return response;
                }
                
                var userIdClaim = tokenprincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? 
                                 tokenprincipal.FindFirstValue(JwtRegisteredClaimNames.Sub);
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Không thể lấy thông tin user từ token.";
                    return response;
                }
                
                if (userId != existingToken.UserId)
                {
                    response.Success = false;
                    response.StatusCode = 401;
                    response.Message = "Refresh token không thuộc về user này.";
                    return response;
                }
                
                // Lấy thông tin user 
                var userinfor = await _userRepository.GetByIdAsync(userId);
                if (userinfor == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "User không tồn tại.";
                    return response;
                }
                
                // Vô hiệu hóa refresh token cũ
                existingToken.IsRevoked = true;
                await _refreshTokenRepository.UpdateAsync(existingToken);
                
                // Tạo tokens mới
                var newRT = GenerateRefreshToken(userinfor);
                var newAT = GenerateAccessToken(userinfor);
                
                // Thêm refresh token mới
                await _refreshTokenRepository.AddAsync(newRT);
                
                response.Data = new RefreshTokenResponseDto
                {
                    AccessToken = newAT.Item1,
                    RefreshToken = newRT.Token,
                    ExpiresAt = newAT.Item2
                };
                
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Refresh token created successfully.";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"An error occurred: {ex.Message}";
                return response;
            }
        }
    }
}