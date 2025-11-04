using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;

namespace CleanDemo.API.Controller.User
{
    [ApiController]
    [Route("api/user/lessons/")]
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


        // Lấy danh sách bài học theo CourseId (chỉ user đã enroll mới xem được)
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
            try
            {
                // Lấy UserId từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

               
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User role not found" });
                }

          
                var response = await _lessonService.GetListLessonByCourseId(courseId, userId, userRole);

                if (!response.Success)
                {
                    if (response.StatusCode == 404)
                    {
                        return NotFound(new { message = response.Message });
                    }
                    if (response.StatusCode == 403)
                    {
                        return StatusCode(403, new { message = response.Message });
                    }
                    return BadRequest(new { message = response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons for course {CourseId}", courseId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User role not found" });
                }

                var response = await _lessonService.GetLessonById(lessonId, userId, userRole);
                if (!response.Success)
                {
                    if (response.StatusCode == 404)
                    {
                        return NotFound(new { message = response.Message });
                    }
                    if (response.StatusCode == 403)
                    {
                        return StatusCode(403, new { message = response.Message });
                    }
                    return BadRequest(new { message = response.Message });

                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson {LessonId}", lessonId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}