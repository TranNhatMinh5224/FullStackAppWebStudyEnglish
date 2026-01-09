using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
    {
        private readonly AppDbContext _context;

        public EmailVerificationTokenRepository(AppDbContext context) => _context = context;

        public async Task<EmailVerificationToken?> GetByEmailAsync(string email) =>
            await _context.EmailVerificationTokens
                .Where(t => t.Email == email && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

        public async Task<List<EmailVerificationToken>> GetAllByEmailAsync(string email) =>
            await _context.EmailVerificationTokens
                .Where(t => t.Email == email)
                .ToListAsync();

        public async Task<EmailVerificationToken?> GetByOtpCodeAsync(string email, string otpCode) =>
            await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.Email == email && t.OtpCode == otpCode && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

        public async Task<EmailVerificationToken?> GetLatestByEmailAsync(string email) =>
            await _context.EmailVerificationTokens
                .Where(t => t.Email == email && !t.IsUsed)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

        public async Task AddAsync(EmailVerificationToken token) =>
            await _context.EmailVerificationTokens.AddAsync(token);

        public async Task UpdateAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Update(token);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Remove(token);
            await Task.CompletedTask;
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _context.EmailVerificationTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsUsed)
                .ToListAsync();

            _context.EmailVerificationTokens.RemoveRange(expiredTokens);
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}
