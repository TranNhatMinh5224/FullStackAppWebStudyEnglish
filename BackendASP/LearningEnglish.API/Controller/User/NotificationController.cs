using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    /// <summary>
    /// Controller đơn giản CHỈ cho việc hiển thị notification từ vựng
    /// 3 API endpoints cơ bản: Get, Get Count, Mark Read
    /// </summary>
    [ApiController]
    [Route("api/notifications")]
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

        /// <summary>
        /// GET: api/notifications - Lấy danh sách thông báo (50 cái gần nhất)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            try
            {
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
                return Ok(new { 
                    isSuccess = true,
                    data = notifications,
                    message = "Success"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { 
                    isSuccess = false,
                    message = "Lỗi khi lấy danh sách thông báo"
                });
            }
        }

        /// <summary>
        /// GET: api/notifications/unread-count - Đếm thông báo chưa đọc (cho badge)
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                return Ok(new { 
                    isSuccess = true,
                    data = count,
                    message = "Success"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { 
                    isSuccess = false,
                    message = "Lỗi khi đếm thông báo chưa đọc"
                });
            }
        }

        /// <summary>
        /// PUT: api/notifications/{id}/mark-as-read - Đánh dấu thông báo đã đọc
        /// </summary>
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            try
            {
                await _notificationRepository.MarkAsReadAsync(id, userId);
                return Ok(new { 
                    isSuccess = true,
                    data = default(object?),
                    message = "Đã đánh dấu thông báo đã đọc"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { 
                    isSuccess = false,
                    message = "Lỗi khi đánh dấu đã đọc"
                });
            }
        }
    }
}