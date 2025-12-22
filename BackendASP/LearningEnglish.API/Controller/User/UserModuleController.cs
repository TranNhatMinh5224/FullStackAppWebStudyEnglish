using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/modules")]
    [ApiController]
    [Authorize]
    public class UserModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly ILogger<UserModuleController> _logger;

        public UserModuleController(
            IModuleService moduleService, 
            IModuleProgressService moduleProgressService,
            ILogger<UserModuleController> logger)
        {
            _moduleService = moduleService;
            _moduleProgressService = moduleProgressService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: api/user/UserModule/{moduleId} - lấy module với tiến độ của user
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModuleWithProgress(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModuleWithProgressAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/UserModule/lesson/{lessonId} - Get all modules in lesson with user progress
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesWithProgress(int lessonId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModulesWithProgressAsync(lessonId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/modules/{moduleId}/start - Vào module (auto-complete cho FlashCard/Lecture/Video/Reading)
        [HttpPost("{moduleId}/start")]
        public async Task<IActionResult> StartModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
