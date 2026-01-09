using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using AutoMapper;

namespace LearningEnglish.Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<NotificationDto>>();
            try
            {
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
                var notificationDtos = _mapper.Map<IEnumerable<NotificationDto>>(notifications);
                response.Success = true;
                response.StatusCode = 200;
                response.Data = notificationDtos;
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
                // Repository sẽ check userId để đảm bảo user chỉ có thể mark read notification của chính mình
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Data = false;
                    response.Message = "Không tìm thấy thông báo";
                    return response;
                }

                // Explicit check: user chỉ có thể mark read notification của chính mình
                if (notification.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Data = false;
                    response.Message = "Bạn không có quyền đánh dấu thông báo này";
                    return response;
                }

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

