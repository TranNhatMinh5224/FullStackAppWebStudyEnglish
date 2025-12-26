using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // GET: api/user/UserModule/{moduleId} - lấy module với tiến độ của user
        // RLS: modules_policy_student_select_enrolled, modulecompletions_policy_user_all_own
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModuleWithProgress(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _moduleService.GetModuleWithProgressAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/UserModule/lesson/{lessonId} - Get all modules in lesson with user progress
        // RLS: modules_policy_student_select_enrolled (chỉ xem modules của enrolled courses)
        // RLS: modulecompletions_policy_user_all_own (chỉ xem completions của chính mình)
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesWithProgress(int lessonId)
        {
            // RLS sẽ filter modules theo enrollment và completions theo userId
            var userId = User.GetUserId();
            var result = await _moduleService.GetModulesWithProgressAsync(lessonId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/modules/{moduleId}/start - Vào module (auto-complete cho FlashCard/Lecture/Video/Reading)
        // RLS: modulecompletions_policy_user_all_own (chỉ tạo completions cho chính mình)
        [HttpPost("{moduleId}/start")]
        public async Task<IActionResult> StartModule(int moduleId)
        {
            // RLS sẽ filter module completions theo userId
            var userId = User.GetUserId();
            var result = await _moduleProgressService.StartAndCompleteModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
