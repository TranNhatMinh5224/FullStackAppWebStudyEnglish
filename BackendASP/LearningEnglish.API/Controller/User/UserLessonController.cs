using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/lessons")]
    [Authorize]
    public class UserLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseRepository _courseRepository;
        private readonly ILogger<UserLessonController> _logger;

        public UserLessonController(ILessonService lessonService, ICourseRepository courseRepository, ILogger<UserLessonController> logger)
        {
            _lessonService = lessonService;
            _courseRepository = courseRepository;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return 0;
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            var userRole = User.GetPrimaryRole();
            if (string.IsNullOrEmpty(userRole))
            {
                return string.Empty;
            }
            return userRole;
        }

        // GET: api/user/lessons/course/{courseId} - lấy danh sách lesson theo course ID
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _lessonService.GetListLessonByCourseId(courseId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/lessons/{lessonId} - lấy thông tin chi tiết của một lesson cụ thể
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            _logger.LogInformation("🔍 GetLessonById called: LessonId={LessonId}, UserId={UserId}, Role={Role}",
                lessonId, userId, userRole);

            var result = await _lessonService.GetLessonById(lessonId, userId, userRole);

            if (result.Success && result.Data != null)
            {
                _logger.LogInformation("Lesson {LessonId} returned for User {UserId} (Role: {Role})",
                    lessonId, userId, userRole);
            }
            else
            {
                _logger.LogWarning("Lesson {LessonId} NOT returned for User {UserId} (Role: {Role}). Status: {StatusCode}, Message: {Message}",
                    lessonId, userId, userRole, result.StatusCode, result.Message);
            }

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
