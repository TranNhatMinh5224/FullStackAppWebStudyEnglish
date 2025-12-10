using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId);
        Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId);
        Task<int> CountRecentTokensByUserIdAsync(int userId, int minutes); // Đếm số OTP gửi trong X phút
        Task AddAsync(PasswordResetToken resetToken);
        Task UpdateAsync(PasswordResetToken resetToken);
        Task DeleteAsync(PasswordResetToken resetToken); // Xóa OTP sau khi verify thành công
        Task DeleteExpiredTokensAsync(); // Xóa tất cả OTP hết hạn
        Task SaveChangesAsync();
    }
}
