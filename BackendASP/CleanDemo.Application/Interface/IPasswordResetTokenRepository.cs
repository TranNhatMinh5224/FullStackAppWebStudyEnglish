using CleanDemo.Domain.Entities;

namespace CleanDemo.Application.Interface
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId);
        Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId);
        Task AddAsync(PasswordResetToken resetToken);
        Task UpdateAsync(PasswordResetToken resetToken);
        Task DeleteExpiredTokensAsync();
        Task SaveChangesAsync();
    }
}
