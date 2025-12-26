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

        // RLS đã filter: User chỉ xem streak của chính mình, Admin xem tất cả
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng khi Admin query streak của user cụ thể
        public async Task<Streak?> GetByUserIdAsync(int userId)
        {
            // RLS đã filter: User chỉ query được streak của chính mình
            // Admin có permission có thể query streak của bất kỳ user nào
            // Filter theo userId để đảm bảo đúng user được query
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

        // RLS đã filter: User chỉ check streak của chính mình
        public async Task<bool> ExistsAsync(int userId)
        {
            // RLS đã filter: User chỉ check được streak của chính mình
            // Filter theo userId để đảm bảo đúng
            return await _context.Streaks.AnyAsync(s => s.UserId == userId);
        }

        // Admin method: Lấy danh sách users có nguy cơ mất streak
        // RLS: Admin có permission Admin.User.Manage mới xem được
        public async Task<List<Streak>> GetUsersAtRiskOfLosingStreakAsync(int minStreak = 3)
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            
            // RLS đã filter: Chỉ Admin có permission mới xem được
            return await _context.Streaks
                .Include(s => s.User)
                .Where(s => s.CurrentStreak >= minStreak 
                    && s.LastActivityDate.HasValue 
                    && s.LastActivityDate.Value.Date == yesterday)
                .ToListAsync();
        }
    }
}
