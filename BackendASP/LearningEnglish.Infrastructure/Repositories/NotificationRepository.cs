using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        // ✅ TẠO THÔNG BÁO - Chức năng chính
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        // ✅ LẤY DANH SÁCH THÔNG BÁO - Đơn giản nhất
        // RLS đã filter: User chỉ xem notifications của chính mình, Admin xem tất cả
        // Defense in depth: Vẫn filter theo userId để đảm bảo đúng
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        {
            // RLS đã filter: User chỉ query được notifications của chính mình
            // Filter theo userId để đảm bảo đúng (defense in depth)
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30) // Giới hạn 30 thông báo gần nhất cho dropdown
                .ToListAsync();
        }

        // RLS đã filter: User chỉ đếm notifications của chính mình
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            // RLS đã filter: User chỉ đếm được notifications của chính mình
            // Filter theo userId để đảm bảo đúng
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        // RLS đã filter: User chỉ update notifications của chính mình
        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            // RLS đã filter: User chỉ update được notifications của chính mình
            // Filter theo userId để đảm bảo đúng
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // RLS đã filter: User chỉ update notifications của chính mình
        public async Task MarkAllAsReadAsync(int userId)
        {
            // RLS đã filter: User chỉ update được notifications của chính mình
            // Filter theo userId để đảm bảo đúng
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}