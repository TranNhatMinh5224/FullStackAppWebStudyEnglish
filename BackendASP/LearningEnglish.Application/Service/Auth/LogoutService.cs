using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.Auth
{
    public class LogoutService : ILogoutService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ILogger<LogoutService> _logger;

        public LogoutService(
            IRefreshTokenRepository refreshTokenRepository,
            ILogger<LogoutService> logger)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _logger = logger;
        }

        // Đăng xuất khỏi device hiện tại
        public async Task<ServiceResponse<object>> LogoutAsync(LogoutDto dto, int userId)
        {
            var response = new ServiceResponse<object>();
            
            try
            {
                // Validate refresh token
                if (string.IsNullOrEmpty(dto.RefreshToken))
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Refresh token là bắt buộc";
                    return response;
                }

                // Kiểm tra token có tồn tại và thuộc về user không
                var existingToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);
                if (existingToken == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Refresh token không tồn tại";
                    return response;
                }

                if (existingToken.UserId != userId)
                {
                    _logger.LogWarning("User {UserId} attempted to revoke token belonging to User {TokenUserId}", userId, existingToken.UserId);
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền thu hồi token này";
                    return response;
                }

                // Revoke token
                await _refreshTokenRepository.RevokeTokenAsync(dto.RefreshToken);

                _logger.LogInformation("User {UserId} logged out successfully from device", userId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đăng xuất thành công";
                response.Data = new { LoggedOut = true };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout for User {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống khi đăng xuất";
                return response;
            }
        }

        // Đăng xuất khỏi tất cả devices
        public async Task<ServiceResponse<object>> LogoutAllAsync(int userId)
        {
            var response = new ServiceResponse<object>();
            
            try
            {
                // Revoke tất cả tokens của user
                await _refreshTokenRepository.RevokeAllTokensForUserAsync(userId);

                _logger.LogInformation("User {UserId} logged out from all devices successfully", userId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đăng xuất khỏi tất cả thiết bị thành công";
                response.Data = new { LoggedOutFromAllDevices = true };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during logout all devices for User {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống khi đăng xuất khỏi tất cả thiết bị";
                return response;
            }
        }
    }
}
