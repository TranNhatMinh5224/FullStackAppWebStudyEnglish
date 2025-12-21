using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface INotificationRepository
    {
        // Tạo thông báo
        Task AddAsync(Notification notification);
        
        // Lấy danh sách thông báo (mới nhất đến cũ nhất, giới hạn 30)
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        
        // Đếm thông báo chưa đọc (hiển thị badge)
        Task<int> GetUnreadCountAsync(int userId);
        
        // Đánh dấu đã đọc
        Task MarkAsReadAsync(int notificationId, int userId);
        
        // Đánh dấu tất cả đã đọc
        Task MarkAllAsReadAsync(int userId);
    }
}
