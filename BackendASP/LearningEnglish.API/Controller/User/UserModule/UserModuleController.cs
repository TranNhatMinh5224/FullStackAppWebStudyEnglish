using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// User xem modules và tiến độ học tập

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/modules")]
    [ApiController]
    [Authorize]
    public class UserModuleController : ControllerBase
    {
        private readonly IUserModuleService _moduleService;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly ILogger<UserModuleController> _logger;

        public UserModuleController(
            IUserModuleService moduleService, 
            IModuleProgressService moduleProgressService,
            ILogger<UserModuleController> logger)
        {
            _moduleService = moduleService;
            _moduleProgressService = moduleProgressService;
            _logger = logger;
        }

        // GET: User xem chi tiết module + progress
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModuleWithProgress(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _moduleService.GetModuleWithProgress(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: User lấy danh sách modules theo lesson + progress
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesWithProgress(int lessonId)
        {
            var userId = User.GetUserId();
            var result = await _moduleService.GetModulesWithProgress(lessonId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: User bắt đầu module (auto-complete cho FlashCard/Lecture/Video/Reading)
        [HttpPost("{moduleId}/start")]
        public async Task<IActionResult> StartModule(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
