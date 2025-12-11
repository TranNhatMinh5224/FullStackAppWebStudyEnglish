using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
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
                throw new UnauthorizedAccessException("Invalid user credentials");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
            {
                throw new UnauthorizedAccessException("User role not found");
            }
            return userRole;
        }

        // GET: api/user/lessons/course/{courseId} - Get all lessons for a course with progress (for enrolled users)
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _lessonService.GetListLessonByCourseId(courseId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/lessons/{lessonId} - Get detailed information about a specific lesson
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            var result = await _lessonService.GetLessonById(lessonId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
