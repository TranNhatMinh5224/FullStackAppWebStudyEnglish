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
    [Route("api/user/progress")]
    [ApiController]
    [Authorize]
    public class UserProgressController : ControllerBase
    {
        private readonly IProgressDashboardService _dashboardService;
        private readonly ICourseProgressService _courseProgressService;
        private readonly IModuleProgressService _moduleProgressService;

        public UserProgressController(
            IProgressDashboardService dashboardService,
            ICourseProgressService courseProgressService,
            IModuleProgressService moduleProgressService)
        {
            _dashboardService = dashboardService;
            _courseProgressService = courseProgressService;
            _moduleProgressService = moduleProgressService;
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

                var dashboardResponse = await _dashboardService.GetUserProgressDashboardAsync(userId);

                if (!dashboardResponse.Success)
                {
                    return BadRequest(dashboardResponse);
                }

                return Ok(new ServiceResponse<UserProgressDashboardDto>
                {
                    Success = true,
                    Message = "Progress dashboard retrieved successfully",
                    Data = dashboardResponse.Data
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

                var courseProgressResponse = await _courseProgressService.GetCourseProgressDetailAsync(userId, courseId);

                if (!courseProgressResponse.Success)
                {
                    return BadRequest(courseProgressResponse);
                }

                return Ok(new ServiceResponse<CourseProgressDetailDto>
                {
                    Success = true,
                    Message = "Course progress retrieved successfully",
                    Data = courseProgressResponse.Data
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

                var statisticsResponse = await _dashboardService.GetProgressStatisticsAsync(userId);

                if (!statisticsResponse.Success)
                {
                    return BadRequest(statisticsResponse);
                }

                return Ok(new ServiceResponse<ProgressStatisticsDto>
                {
                    Success = true,
                    Message = "Statistics retrieved successfully",
                    Data = statisticsResponse.Data
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

                var startResponse = await _moduleProgressService.StartModuleAsync(userId, moduleId);

                if (!startResponse.Success)
                {
                    return BadRequest(startResponse);
                }

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

                var completeResponse = await _moduleProgressService.CompleteModuleAsync(userId, moduleId);

                if (!completeResponse.Success)
                {
                    return BadRequest(completeResponse);
                }

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
