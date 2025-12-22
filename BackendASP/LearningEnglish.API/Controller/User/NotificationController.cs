using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    // quản lý thông báo cho User
    [ApiController]
    [Route("api/user/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;

        public NotificationController(INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }

        // GET: api/user/notifications - lấy danh sách thông báo của user (mới nhất đến cũ nhất)
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetCurrentUserId();
            var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
            return Ok(new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Data = notifications,
                Message = "Success"
            });
        }

        // GET: api/user/notifications/unread-count - đếm thông báo chưa đọc
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _notificationRepository.GetUnreadCountAsync(userId);
            return Ok(new ServiceResponse<int>
            {
                Success = true,
                StatusCode = 200,
                Data = count,
                Message = "Success"
            });
        }

        // PUT: api/user/notifications/{id}/mark-as-read - đánh dấu 1 thông báo đã đọc
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            await _notificationRepository.MarkAsReadAsync(id, userId);
            return Ok(new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = "Đã đánh dấu đã đọc"
            });
        }

        // PUT: api/user/notifications/mark-all-read - đánh dấu tất cả đã đọc
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            await _notificationRepository.MarkAllAsReadAsync(userId);
            return Ok(new ServiceResponse<object>
            {
                Success = true,
                StatusCode = 200,
                Message = "Đã đánh dấu tất cả đã đọc"
            });
        }
    }
}