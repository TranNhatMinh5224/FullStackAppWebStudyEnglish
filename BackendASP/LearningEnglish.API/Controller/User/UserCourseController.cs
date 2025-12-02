using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/courses")]
    public class UserCourseController : ControllerBase
    {
        private readonly IUserCourseService _userCourseService;
        private readonly ILogger<UserCourseController> _logger;

        public UserCourseController(IUserCourseService userCourseService, ILogger<UserCourseController> logger)
        {
            _userCourseService = userCourseService;
            _logger = logger;
        }

        // GET: api/user/courses/system-courses - Retrieve all available system courses (public access, optionally authenticated)
        [HttpGet("system-courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemCourses()
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var result = await _userCourseService.GetSystemCoursesAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/courses/{courseId} - Retrieve course details by ID with enrollment status
        [HttpGet("{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
