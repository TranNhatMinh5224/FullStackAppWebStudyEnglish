using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IEmailVerificationTokenRepository
    {
        Task<EmailVerificationToken?> GetByEmailAsync(string email); // Get email verification token by email
        Task<List<EmailVerificationToken>> GetAllByEmailAsync(string email); // Get all tokens của email này
        Task AddAsync(EmailVerificationToken token);
        Task UpdateAsync(EmailVerificationToken token); // Update token (for AttemptsCount, BlockedUntil)
        Task DeleteAsync(EmailVerificationToken token);
        Task DeleteExpiredTokensAsync();
        Task SaveChangesAsync();
    }
}
