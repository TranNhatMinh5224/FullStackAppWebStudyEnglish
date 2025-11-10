using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly AppDbContext _context;

        public PasswordResetTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token);
        }

        public async Task<PasswordResetToken?> GetActiveTokenByUserIdAsync(int userId)
        {
            return await _context.PasswordResetTokens
                .Where(prt => prt.UserId == userId && 
                             prt.ExpiresAt > DateTime.UtcNow && 
                             !prt.IsUsed)
                .OrderByDescending(prt => prt.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<PasswordResetToken>> GetActiveTokensByUserIdAsync(int userId)
        {
            return await _context.PasswordResetTokens
                .Where(prt => prt.UserId == userId && 
                             prt.ExpiresAt > DateTime.UtcNow && 
                             !prt.IsUsed)
                .ToListAsync();
        }

        public async Task AddAsync(PasswordResetToken passwordResetToken)
        {
            await _context.PasswordResetTokens.AddAsync(passwordResetToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PasswordResetToken passwordResetToken)
        {
            _context.PasswordResetTokens.Update(passwordResetToken);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExpiredTokensAsync()
        {
            var expiredTokens = await _context.PasswordResetTokens
                .Where(prt => prt.ExpiresAt < DateTime.UtcNow || prt.IsUsed)
                .ToListAsync();

            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync();
        }
    }
}
