using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        // GET: api/user/streaks - lấy chuỗi ngày học hiện tại của user
        [HttpGet]
        public async Task<IActionResult> GetCurrentStreak()
        {
            var userId = GetCurrentUserId();
            var result = await _streakService.GetCurrentStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/streaks/checkin - cập nhật streak khi user online
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckInStreak()
        {
            var userId = GetCurrentUserId();
            var result = await _streakService.UpdateStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
