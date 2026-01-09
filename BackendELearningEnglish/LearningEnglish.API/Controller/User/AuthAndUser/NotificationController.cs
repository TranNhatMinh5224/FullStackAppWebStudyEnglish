using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // GET: api/user/notifications - lấy danh sách thông báo của user (chỉ của chính mình)
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.GetUserId();
            var result = await _notificationService.GetUserNotificationsAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/notifications/unread-count - đếm thông báo chưa đọc (chỉ của chính mình)
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = User.GetUserId();
            var result = await _notificationService.GetUnreadCountAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/notifications/{id}/mark-as-read - đánh dấu 1 thông báo đã đọc (chỉ của chính mình)
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.GetUserId();
            var result = await _notificationService.MarkAsReadAsync(id, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/notifications/mark-all-read - đánh dấu tất cả đã đọc (chỉ của chính mình)
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.GetUserId();
            var result = await _notificationService.MarkAllAsReadAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}