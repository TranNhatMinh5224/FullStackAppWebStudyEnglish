using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface INotificationService
    {
        Task<ServiceResponse<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(int userId);
        Task<ServiceResponse<int>> GetUnreadCountAsync(int userId);
        Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        Task<ServiceResponse<bool>> MarkAllAsReadAsync(int userId);
    }
}

