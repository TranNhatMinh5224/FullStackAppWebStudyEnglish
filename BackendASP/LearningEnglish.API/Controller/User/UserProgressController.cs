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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }
            return userId;
        }

        // GET: api/user/progress/dashboard - Get comprehensive progress dashboard with all courses and statistics
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetProgressDashboard()
        {
            var userId = GetCurrentUserId();
            var result = await _dashboardService.GetUserProgressDashboardAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/progress/course/{courseId} - Get detailed progress for a specific course including modules
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseProgress(int courseId)
        {
            var userId = GetCurrentUserId();
            var result = await _courseProgressService.GetCourseProgressDetailAsync(userId, courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/progress/statistics - Get progress statistics (completion rates, time spent, etc.)
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var userId = GetCurrentUserId();
            var result = await _dashboardService.GetProgressStatisticsAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/progress/module/{moduleId}/start - Mark a module as started when user opens it
        [HttpPost("module/{moduleId}/start")]
        public async Task<IActionResult> StartModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleProgressService.StartModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/progress/module/{moduleId}/complete - Mark a module as completed when user finishes all content
        [HttpPost("module/{moduleId}/complete")]
        public async Task<IActionResult> CompleteModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleProgressService.CompleteModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
