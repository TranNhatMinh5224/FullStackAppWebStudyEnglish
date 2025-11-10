using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/[controller]")]
    [ApiController]
    [Authorize]
    public class UserModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ILogger<UserModuleController> _logger;

        public UserModuleController(IModuleService moduleService, ILogger<UserModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Get module with user progress
        /// </summary>
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModuleWithProgress(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModuleWithProgressAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get all modules in a lesson with progress
        /// </summary>
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesWithProgress(int lessonId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModulesWithProgressAsync(lessonId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get next module for user
        /// </summary>
        [HttpGet("{moduleId}/next")]
        public async Task<IActionResult> GetNextModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetNextModuleAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get previous module for user
        /// </summary>
        [HttpGet("{moduleId}/previous")]
        public async Task<IActionResult> GetPreviousModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetPreviousModuleAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Check if user can access module
        /// </summary>
        [HttpGet("{moduleId}/access")]
        public async Task<IActionResult> CheckAccess(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.CanUserAccessModuleAsync(moduleId, userId);

            return Ok(result);
        }
    }
}
