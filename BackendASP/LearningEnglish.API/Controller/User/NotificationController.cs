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
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    StatusCode = 401, 
                    Message = "Invalid user credentials" 
                });

            try
            {
                var notifications = await _notificationRepository.GetUserNotificationsAsync(userId);
                return Ok(new ServiceResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Data = notifications,
                    Message = "Success"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi khi lấy danh sách thông báo"
                });
            }
        }

        // GET: api/user/notifications/unread-count - đếm thông báo chưa đọc
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    StatusCode = 401, 
                    Message = "Invalid user credentials" 
                });

            try
            {
                var count = await _notificationRepository.GetUnreadCountAsync(userId);
                return Ok(new ServiceResponse<int>
                {
                    Success = true,
                    StatusCode = 200,
                    Data = count,
                    Message = "Success"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi khi đếm thông báo"
                });
            }
        }

        // PUT: api/user/notifications/{id}/mark-as-read - đánh dấu 1 thông báo đã đọc
        [HttpPut("{id}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    StatusCode = 401, 
                    Message = "Invalid user credentials" 
                });

            try
            {
                await _notificationRepository.MarkAsReadAsync(id, userId);
                return Ok(new ServiceResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Đã đánh dấu đã đọc"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi khi đánh dấu thông báo"
                });
            }
        }

        // PUT: api/user/notifications/mark-all-read - đánh dấu tất cả đã đọc
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    StatusCode = 401, 
                    Message = "Invalid user credentials" 
                });

            try
            {
                await _notificationRepository.MarkAllAsReadAsync(userId);
                return Ok(new ServiceResponse<object>
                {
                    Success = true,
                    StatusCode = 200,
                    Message = "Đã đánh dấu tất cả đã đọc"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    StatusCode = 500,
                    Message = "Lỗi khi đánh dấu thông báo"
                });
            }
        }
    }
}