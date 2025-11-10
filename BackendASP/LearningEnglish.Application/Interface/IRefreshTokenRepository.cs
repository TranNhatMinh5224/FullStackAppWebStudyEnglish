using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IRefreshTokenRepository
    {
        //  lấy RefreshToken dựa trên token
        Task<RefreshToken?> GetByTokenAsync(string token);

        // lấy tất cả RefreshToken của một user dựa trên userId
        Task<List<RefreshToken>> GetTokensByUserIdAsync(int userId);

        // thêm mới một RefreshToken
        Task AddAsync(RefreshToken refreshToken);

        // cập nhật một RefreshToken
        Task UpdateAsync(RefreshToken refreshToken);

        // xóa một RefreshToken dựa trên token
        Task DeleteAsync(string token);

        // lưu thay đổi vào database
        Task SaveChangesAsync();
        // xóa tất cả RefreshToken đã hết hạn
        Task DeleteExpiredTokensAsync();
    }
}
