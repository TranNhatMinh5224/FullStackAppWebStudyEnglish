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

        // Lấy thông tin module với tiến độ của user
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

        // Lấy tất cả module trong lesson với tiến độ
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
    }
}
