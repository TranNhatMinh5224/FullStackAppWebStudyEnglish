using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Interface
{
	public interface INotificationService
	{
		Task<ServiceResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto request);

		Task<ServiceResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, int pageNumber = 1, int pageSize = 20);

		Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, int userId);

		Task<ServiceResponse<bool>> MarkAllAsReadAsync(int userId);

		Task<ServiceResponse<bool>> SendEmailNotificationAsync(int notificationId);

		Task<ServiceResponse<bool>> DeleteNotificationAsync(int notificationId, int userId);

		Task<ServiceResponse<int>> GetUnreadCountAsync(int userId);
	}
}
