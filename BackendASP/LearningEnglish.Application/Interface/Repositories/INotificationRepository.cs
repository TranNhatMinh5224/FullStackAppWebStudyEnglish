using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface INotificationRepository
    {
        // Tạo thông báo
        Task AddAsync(Notification notification);
        
        // Lấy danh sách thông báo
        // RLS: User chỉ xem notifications của chính mình, Admin xem tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        
        // Đếm thông báo chưa đọc
        // RLS: User chỉ đếm notifications của chính mình, Admin đếm tất cả (có permission)
        // userId parameter: Defense in depth (RLS + userId filter)
        Task<int> GetUnreadCountAsync(int userId);
        
        // Đánh dấu đã đọc
        // RLS: User chỉ update notifications của chính mình
        // userId parameter: Defense in depth (RLS + userId filter)
        Task MarkAsReadAsync(int notificationId, int userId);
        
        // Đánh dấu tất cả đã đọc
        // RLS: User chỉ update notifications của chính mình
        // userId parameter: Defense in depth (RLS + userId filter)
        Task MarkAllAsReadAsync(int userId);
    }
}
