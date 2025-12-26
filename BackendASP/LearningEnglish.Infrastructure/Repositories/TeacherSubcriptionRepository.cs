using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace LearningEnglish.Infrastructure.Repositories
{
    public class TeacherSubscriptionRepository : ITeacherSubscriptionRepository
    {
        private readonly AppDbContext _context;

        public TeacherSubscriptionRepository(AppDbContext context)
        {
            _context = context;
        }

        // RLS đã filter: Teacher chỉ tạo subscriptions cho chính mình, Admin có thể tạo cho bất kỳ teacher nào
        public async Task AddTeacherSubscriptionAsync(TeacherSubscription teacherSubscription)
        {
            _context.TeacherSubscriptions.Add(teacherSubscription);
            await _context.SaveChangesAsync();
        }

        // RLS đã filter: Teacher chỉ xóa subscriptions của chính mình, Admin xóa được tất cả
        public async Task DeleteTeacherSubscriptionAsync(TeacherSubscription IdSubcription)
        {
            _context.TeacherSubscriptions.Remove(IdSubcription);
            await _context.SaveChangesAsync();
        }

        // RLS đã filter: Teacher chỉ xem subscriptions của chính mình, Admin xem tất cả
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng khi query subscription của teacher cụ thể
        public async Task<TeacherSubscription?> GetActiveSubscriptionAsync(int userId)
        {
            var now = DateTime.UtcNow;
            // RLS đã filter: Teacher chỉ query được subscriptions của chính mình
            // Admin có permission có thể query subscriptions của bất kỳ teacher nào
            // Filter theo userId để đảm bảo đúng (defense in depth)
            return await _context.TeacherSubscriptions
                .Include(ts => ts.TeacherPackage)
                .Where(ts => ts.UserId == userId
                          && ts.Status == Domain.Enums.SubscriptionStatus.Active
                          && ts.EndDate > now)
                .OrderByDescending(ts => ts.EndDate)
                .FirstOrDefaultAsync();
        }
    }
}
