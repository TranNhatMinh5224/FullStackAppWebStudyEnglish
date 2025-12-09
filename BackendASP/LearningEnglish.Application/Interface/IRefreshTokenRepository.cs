using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IRefreshTokenRepository
    {
        // Lấy RefreshToken dựa trên token
        Task<RefreshToken?> GetByTokenAsync(string token);

        // Lấy tất cả RefreshToken của một user
        Task<List<RefreshToken>> GetTokensByUserIdAsync(int userId);

        // Thêm mới RefreshToken
        Task AddAsync(RefreshToken refreshToken);

        // Cập nhật RefreshToken
        Task UpdateAsync(RefreshToken refreshToken);

        // Xóa RefreshToken dựa trên token
        Task DeleteAsync(string token);

        // Lưu thay đổi vào database
        Task SaveChangesAsync();
        
        // Xóa tất cả RefreshToken đã hết hạn
        Task DeleteExpiredTokensAsync();
        
        // Thu hồi tất cả tokens của user (security)
        Task RevokeAllTokensForUserAsync(int userId);
        
        // Thu hồi token cụ thể (logout)
        Task RevokeTokenAsync(string token);
    }
}
