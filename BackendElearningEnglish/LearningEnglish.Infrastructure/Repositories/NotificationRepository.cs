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

        //  TẠO THÔNG BÁO - Chức năng chính
        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        // Lấy thông báo theo ID
        public async Task<Notification?> GetByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);
        }

        
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        {
          
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(30) // Giới hạn 30 thông báo gần nhất cho dropdown
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

       
        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

   
        public async Task MarkAllAsReadAsync(int userId)
        {
            
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