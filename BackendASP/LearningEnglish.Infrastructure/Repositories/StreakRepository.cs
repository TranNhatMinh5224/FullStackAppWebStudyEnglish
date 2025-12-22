using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class StreakRepository : IStreakRepository
    {
        private readonly AppDbContext _context;

        public StreakRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Streak?> GetByUserIdAsync(int userId)
        {
            return await _context.Streaks
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Streak> CreateAsync(Streak streak)
        {
            await _context.Streaks.AddAsync(streak);
            await _context.SaveChangesAsync();
            return streak;
        }

        public async Task UpdateAsync(Streak streak)
        {
            _context.Streaks.Update(streak);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int userId)
        {
            return await _context.Streaks.AnyAsync(s => s.UserId == userId);
        }

        public async Task<List<Streak>> GetUsersAtRiskOfLosingStreakAsync(int minStreak = 3)
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            
            return await _context.Streaks
                .Include(s => s.User)
                .Where(s => s.CurrentStreak >= minStreak 
                    && s.LastActivityDate.HasValue 
                    && s.LastActivityDate.Value.Date == yesterday)
                .ToListAsync();
        }
    }
}
