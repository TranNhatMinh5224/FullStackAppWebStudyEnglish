using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/streaks")]
    [ApiController]
    [Authorize]
    public class StreakController : ControllerBase
    {
        private readonly IStreakService _streakService;

        public StreakController(IStreakService streakService)
        {
            _streakService = streakService;
        }

        // GET: api/user/streaks - lấy chuỗi ngày học hiện tại của user
       
        [HttpGet]
        public async Task<IActionResult> GetCurrentStreak()
        {
            var userId = User.GetUserId();
            var result = await _streakService.GetCurrentStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/streaks/checkin - cập nhật streak khi user online
       
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckInStreak()
        {
            var userId = User.GetUserId();
            var result = await _streakService.UpdateStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/streaks/send-reminders - gửi reminder cho users sắp đứt streak (Admin/Cron job)
        [HttpPost("send-reminders")]
        [AllowAnonymous] 
        public async Task<IActionResult> SendStreakReminders()
        {
            var result = await _streakService.SendStreakRemindersAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
