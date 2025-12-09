using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Domain.Entities;
using AutoMapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailSender emailSender,
            IMapper mapper)
        {
            _notificationRepository = notificationRepository;
            _emailSender = emailSender;
            _mapper = mapper;
        }

        public async Task<ServiceResponse<NotificationDto>> CreateNotificationAsync(CreateNotificationDto request)
        {
            var response = new ServiceResponse<NotificationDto>();

            try
            {
                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Message,
                    Type = request.Type,
                    RelatedEntityType = request.RelatedEntityType,
                    RelatedEntityId = request.RelatedEntityId
                };

                await _notificationRepository.AddAsync(notification);

                // Send email if requested
                if (request.SendEmail)
                {
                    await SendEmailNotificationAsync(notification.Id);
                }

                response.Data = _mapper.Map<NotificationDto>(notification);
                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Tạo thông báo thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi tạo thông báo: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<List<NotificationDto>>> GetUserNotificationsAsync(int userId, bool unreadOnly = false, int pageNumber = 1, int pageSize = 20)
        {
            var response = new ServiceResponse<List<NotificationDto>>();

            try
            {
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId, unreadOnly, pageNumber, pageSize);
                response.Data = _mapper.Map<List<NotificationDto>>(notifications);
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thông báo thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy thông báo: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> MarkAsReadAsync(int notificationId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                await _notificationRepository.MarkAsReadAsync(notificationId, userId);
                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đã đánh dấu thông báo là đã đọc";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi đánh dấu thông báo: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> MarkAllAsReadAsync(int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                await _notificationRepository.MarkAllAsReadAsync(userId);
                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Đã đánh dấu tất cả thông báo là đã đọc";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi đánh dấu tất cả thông báo: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> SendEmailNotificationAsync(int notificationId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy thông báo";
                    return response;
                }



                notification.IsEmailSent = true;
                notification.EmailSentAt = DateTime.UtcNow;
                await _notificationRepository.UpdateAsync(notification);

                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Gửi email thông báo thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi gửi email thông báo: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteNotificationAsync(int notificationId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var notification = await _notificationRepository.GetByIdAsync(notificationId);
                if (notification == null || notification.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy thông báo hoặc không có quyền";
                    return response;
                }

                await _notificationRepository.DeleteAsync(notificationId);
                response.Data = true;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa thông báo thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi xóa thông báo: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<int>> GetUnreadCountAsync(int userId)
        {
            var response = new ServiceResponse<int>();

            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                response.Data = count;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy số lượng thông báo chưa đọc thành công";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy số lượng chưa đọc: {ex.Message}";
            }

            return response;
        }
    }
}