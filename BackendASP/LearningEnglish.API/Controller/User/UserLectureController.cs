using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: api/userlecture/{lectureId} - lấy lecture theo ID
        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            var userId = GetCurrentUserId();
            var result = await _lectureService.GetLectureByIdAsync(lectureId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/userlecture/module/{moduleId} - lấy tất cả lecture theo module ID
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/userlecture/module/{moduleId}/tree - lấy cây lecture theo module ID
        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // ❌ Removed CompleteLecture endpoint
        // Module completion now handled by POST /api/user/modules/{moduleId}/start
        // When user enters module, it will auto-complete for Lecture/Video/Reading types
    }
}
