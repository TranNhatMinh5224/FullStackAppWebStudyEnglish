using LearningEnglish.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
    public interface INotificationRepository
    {
        // ✅ 3 chức năng CƠ BẢN NHẤT
        Task AddAsync(Notification notification);                              // Tạo thông báo
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId); // Lấy danh sách thông báo
        Task<int> GetUnreadCountAsync(int userId);                             // Đếm thông báo chưa đọc
        
        // ✅ Optional: Đánh dấu đã đọc
        Task MarkAsReadAsync(int notificationId, int userId);
    }
}
