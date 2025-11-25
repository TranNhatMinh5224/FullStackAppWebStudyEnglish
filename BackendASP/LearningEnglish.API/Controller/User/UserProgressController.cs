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
    [Route("api/user/progress")]
    [ApiController]
    [Authorize]
    public class UserProgressController : ControllerBase
    {
        private readonly IUserProgressService _progressService;

        public UserProgressController(IUserProgressService progressService)
        {
            _progressService = progressService;
        }

        /// <summary>
        /// Get comprehensive progress dashboard for current user
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetProgressDashboard()
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

                var dashboard = await _progressService.GetUserProgressDashboardAsync(userId);

                return Ok(new ServiceResponse<UserProgressDashboardDto>
                {
                    Success = true,
                    Message = "Progress dashboard retrieved successfully",
                    Data = dashboard
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving progress dashboard: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get detailed progress for a specific course
        /// </summary>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseProgress(int courseId)
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

                var courseProgress = await _progressService.GetCourseProgressDetailAsync(userId, courseId);

                return Ok(new ServiceResponse<CourseProgressDetailDto>
                {
                    Success = true,
                    Message = "Course progress retrieved successfully",
                    Data = courseProgress
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving course progress: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get progress statistics for current user
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
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

                var statistics = await _progressService.GetProgressStatisticsAsync(userId);

                return Ok(new ServiceResponse<ProgressStatisticsDto>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = statistics
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving statistics: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Mark a module as started (called when user opens a module)
        /// </summary>
        [HttpPost("module/{moduleId}/start")]
        public async Task<IActionResult> StartModule(int moduleId)
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

                await _progressService.StartModuleAsync(userId, moduleId);

                return Ok(new ServiceResponse<object>
                {
                    Success = true,
                    Message = "Module started successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error starting module: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Mark a module as completed (called when user completes a module)
        /// </summary>
        [HttpPost("module/{moduleId}/complete")]
        public async Task<IActionResult> CompleteModule(int moduleId)
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

                await _progressService.CompleteModuleAsync(userId, moduleId);

                return Ok(new ServiceResponse<object>
                {
                    Success = true,
                    Message = "Module completed successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponse<object>
                {
                    Success = false,
                    Message = $"Error completing module: {ex.Message}"
                });
            }
        }
    }
}
