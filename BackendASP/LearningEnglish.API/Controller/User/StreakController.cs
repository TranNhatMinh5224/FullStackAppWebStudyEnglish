using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;
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

        /// <summary>
        /// Get current streak information for authenticated user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCurrentStreak()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ServiceResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _streakService.GetCurrentStreakAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving streak: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get longest streak record for authenticated user
        /// </summary>
        [HttpGet("longest")]
        public async Task<IActionResult> GetLongestStreak()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ServiceResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _streakService.GetLongestStreakAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving longest streak: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get streak history for the last N days (default 30)
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetStreakHistory([FromQuery] int days = 30)
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ServiceResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                if (days < 1 || days > 365)
                {
                    return BadRequest(new ServiceResponse<object>
                    {
                        Success = false,
                        Message = "Days must be between 1 and 365"
                    });
                }

                var result = await _streakService.GetStreakHistoryAsync(userId, days);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving streak history: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Reset streak for authenticated user (Admin only or testing)
        /// </summary>
        [HttpPost("reset")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ResetStreak()
        {
            try
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new ServiceResponse<object>
                    {
                        Success = false,
                        Message = "User not authenticated"
                    });
                }

                var result = await _streakService.ResetStreakAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error resetting streak: {ex.Message}"
                });
            }
        }
    }
}
