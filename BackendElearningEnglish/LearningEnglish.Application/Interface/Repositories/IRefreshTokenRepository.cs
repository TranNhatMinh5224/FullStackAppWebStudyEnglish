using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IRefreshTokenRepository
    {
        // Lấy token theo mã token
        Task<RefreshToken?> GetByTokenAsync(string token);

        // Lấy tất cả token của user
        Task<List<RefreshToken>> GetTokensByUserIdAsync(int userId);

        // Thêm refresh token
        Task AddAsync(RefreshToken refreshToken);

        // Cập nhật refresh token
        Task UpdateAsync(RefreshToken refreshToken);

        // Xóa token theo mã token
        Task DeleteAsync(string token);

        // Lưu thay đổi
        Task SaveChangesAsync();
        
        // Xóa token hết hạn
        Task DeleteExpiredTokensAsync();
        
        // Thu hồi tất cả token của user
        Task RevokeAllTokensForUserAsync(int userId);
        
        // Thu hồi token cụ thể
        Task RevokeTokenAsync(string token);
    }
}
