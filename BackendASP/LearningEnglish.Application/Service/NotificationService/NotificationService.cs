using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationService(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Notification>>> GetUserNotificationsAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<Notification>>();
            try
            {
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
                response.Success = true;
                response.StatusCode = 200;
                response.Data = notifications;
                response.Message = "Success";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi lấy danh sách thông báo";
            }
            return response;
        }

        public async Task<ServiceResponse<int>> GetUnreadCountAsync(int userId)
        {
            var response = new ServiceResponse<int>();
            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                response.Success = true;
                response.StatusCode = 200;
                response.Data = count;
                response.Message = "Success";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi đếm thông báo chưa đọc";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId, userId);
                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đã đánh dấu đã đọc";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi đánh dấu đã đọc";
            }
            return response;
        }

        public async Task<ServiceResponse<bool>> MarkAllAsReadAsync(int userId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                await _notificationRepository.MarkAllAsReadAsync(userId);
                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Đã đánh dấu tất cả đã đọc";
            }
            catch (Exception)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi khi đánh dấu tất cả đã đọc";
            }
            return response;
        }
    }
}

