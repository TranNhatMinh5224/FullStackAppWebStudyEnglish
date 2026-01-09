using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IPasswordResetTokenRepository
    {
        // Lấy token theo mã token
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        
        // Lấy token đang active của user
        Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId);
        
        // Lấy tất cả token active của user
        Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId);
        
        // Đếm số token gần đây
        Task<int> CountRecentTokensByUserIdAsync(int userId, int minutes);
        
        // Thêm token
        Task AddAsync(PasswordResetToken resetToken);
        
        // Cập nhật token
        Task UpdateAsync(PasswordResetToken resetToken);
        
        // Xóa token
        Task DeleteAsync(PasswordResetToken resetToken);
        
        // Xóa token hết hạn
        Task DeleteExpiredTokensAsync();
        
        // Lưu thay đổi
        Task SaveChangesAsync();
    }
}
