using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/lectures")]
    [ApiController]
    [Authorize]
    public class UserLectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;
        private readonly ILogger<UserLectureController> _logger;

        public UserLectureController(
            ILectureService lectureService,
            ILogger<UserLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }

        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.GetLectureByIdAsync(lectureId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // ❌ Removed CompleteLecture endpoint
        // Module completion now handled by POST /api/user/modules/{moduleId}/start
        // When user enters module, it will auto-complete for Lecture/Video/Reading types
    }
}
