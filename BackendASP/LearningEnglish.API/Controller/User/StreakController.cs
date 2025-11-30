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
    [Route("api/user/streak")]
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

        // GET: api/user/streak - Get current streak information (days count, last activity)
        [HttpGet]
        public async Task<IActionResult> GetCurrentStreak()
        {
            var userId = GetCurrentUserId();
            var result = await _streakService.GetCurrentStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/streak/longest - Get longest streak record for user
        [HttpGet("longest")]
        public async Task<IActionResult> GetLongestStreak()
        {
            var userId = GetCurrentUserId();
            var result = await _streakService.GetLongestStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/streak/history - Get streak activity history for the last N days (default: 30, max: 365)
        [HttpGet("history")]
        public async Task<IActionResult> GetStreakHistory([FromQuery] int days = 30)
        {
            if (days < 1 || days > 365)
            {
                return BadRequest(new ServiceResponse<object>
                {
                    Success = false,
                    Message = "Days must be between 1 and 365"
                });
            }

            var userId = GetCurrentUserId();
            var result = await _streakService.GetStreakHistoryAsync(userId, days);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/streak/reset - Reset streak to zero (Admin only, for testing purposes)
        [HttpPost("reset")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetStreak()
        {
            var userId = GetCurrentUserId();
            var result = await _streakService.ResetStreakAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
