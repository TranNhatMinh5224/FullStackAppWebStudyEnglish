using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserLectureController : ControllerBase
    {
        private readonly ILectureService _lectureService;
        private readonly ILogger<UserLectureController> _logger;

        public UserLectureController(ILectureService lectureService, ILogger<UserLectureController> logger)
        {
            _lectureService = lectureService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Lấy thông tin lecture theo ID (chỉ đọc)
        /// </summary>
        [HttpGet("{lectureId}")]
        public async Task<IActionResult> GetLecture(int lectureId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _lectureService.GetLectureByIdAsync(lectureId, userId);

                if (!result.Success)
                    return NotFound(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi user lấy lecture với ID: {LectureId}", lectureId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy thông tin lecture");
            }
        }

        /// <summary>
        /// Lấy danh sách lecture theo module ID (chỉ đọc)
        /// </summary>
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _lectureService.GetLecturesByModuleIdAsync(moduleId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi user lấy danh sách lecture theo ModuleId: {ModuleId}", moduleId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy danh sách lecture");
            }
        }

        /// <summary>
        /// Lấy cấu trúc cây lecture theo module ID (chỉ đọc)
        /// </summary>
        [HttpGet("module/{moduleId}/tree")]
        public async Task<IActionResult> GetLectureTree(int moduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _lectureService.GetLectureTreeByModuleIdAsync(moduleId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi user lấy cấu trúc cây lecture theo ModuleId: {ModuleId}", moduleId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy cấu trúc cây lecture");
            }
        }
    }
}
