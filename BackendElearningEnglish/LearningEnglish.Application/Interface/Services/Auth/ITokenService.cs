using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;
using System.Security.Claims;


namespace LearningEnglish.Application.Interface.Auth
{
    public interface ITokenService
    {
        // Tạo Access Token
        public Tuple<string, DateTime> GenerateAccessToken(User user);
        
        // Tạo Refresh Token
        RefreshToken GenerateRefreshToken(User user);

        // Làm mới token
        public Task<ServiceResponse<RefreshTokenResponseDto>> RefreshTokenAsync(ReceiveRefreshTokenDto request);
        
        // Lấy thông tin từ token hết hạn
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
