using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEmailVerificationTokenRepository
    {
        // Lấy token theo email
        Task<EmailVerificationToken?> GetByEmailAsync(string email);
        
        // Lấy tất cả token của email
        Task<List<EmailVerificationToken>> GetAllByEmailAsync(string email);
        
        // Thêm token
        Task AddAsync(EmailVerificationToken token);
        
        // Cập nhật token
        Task UpdateAsync(EmailVerificationToken token);
        
        // Xóa token
        Task DeleteAsync(EmailVerificationToken token);
        
        // Xóa token hết hạn
        Task DeleteExpiredTokensAsync();
        
        // Lưu thay đổi
        Task SaveChangesAsync();
    }
}
