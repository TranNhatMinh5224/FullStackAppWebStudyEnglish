using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface INotificationRepository
    {
        // Tạo thông báo
        Task AddAsync(Notification notification);
        
        // Lấy thông báo theo ID
        Task<Notification?> GetByIdAsync(int notificationId);
        
        // Lấy danh sách thông báo
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        
        // Đếm thông báo chưa đọc
        Task<int> GetUnreadCountAsync(int userId);
        
        // Đánh dấu đã đọc
        Task MarkAsReadAsync(int notificationId, int userId);
        
        // Đánh dấu tất cả đã đọc
        Task MarkAllAsReadAsync(int userId);
    }
}
