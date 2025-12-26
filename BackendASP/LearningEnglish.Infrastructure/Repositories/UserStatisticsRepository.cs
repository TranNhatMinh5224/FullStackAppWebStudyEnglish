using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation cho User Statistics (tách riêng để dễ quản lý và maintain)
    /// </summary>
    public class UserStatisticsRepository : IUserStatisticsRepository
    {
        private readonly AppDbContext _context;

        public UserStatisticsRepository(AppDbContext context)
        {
            _context = context;
        }

        // User count statistics
        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetUserCountByRoleAsync(string roleName)
        {
            return await _context.Users
                .Include(u => u.Roles)
                .Where(u => u.Roles.Any(r => r.Name == roleName))
                .CountAsync();
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users
                .Where(u => u.Status == AccountStatus.Active)
                .CountAsync();
        }

        public async Task<int> GetBlockedUsersCountAsync()
        {
            return await _context.Users
                .Where(u => u.Status == AccountStatus.Suspended || u.Status == AccountStatus.Inactive)
                .CountAsync();
        }

        public async Task<int> GetNewUsersCountAsync(DateTime fromDate)
        {
            return await _context.Users
                .Where(u => u.CreatedAt >= fromDate)
                .CountAsync();
        }
    }
}

