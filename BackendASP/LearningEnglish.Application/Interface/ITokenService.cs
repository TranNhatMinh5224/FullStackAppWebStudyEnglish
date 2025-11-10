using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;
using System.Security.Claims;


namespace LearningEnglish.Application.Interface
{
    public interface ITokenService
    {
        // public (string token, DateTime expiresAt) GenerateAccessToken(User user);
        public Tuple<string, DateTime> GenerateAccessToken(User user); // Lấy cặp (token, expiresAt)
        RefreshToken GenerateRefreshToken(User user); // Tạo mới Refresh Token

        public Task<ServiceResponse<RefreshTokenResponseDto>> RefreshTokenAsync(ReceiveRefreshTokenDto request);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    }
}
