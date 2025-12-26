using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface INotificationService
    {
        Task<ServiceResponse<IEnumerable<Notification>>> GetUserNotificationsAsync(int userId);
        Task<ServiceResponse<int>> GetUnreadCountAsync(int userId);
        Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, int userId);
        Task<ServiceResponse<bool>> MarkAllAsReadAsync(int userId);
    }
}

