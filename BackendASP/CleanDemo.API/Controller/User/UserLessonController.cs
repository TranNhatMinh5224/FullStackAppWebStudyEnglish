using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;

namespace CleanDemo.API.Controller.User
{
    [ApiController]
    [Route("api/user/lessons/")]
    [Authorize]  // Thay đổi từ AllowAnonymous thành Authorize để yêu cầu đăng nhập
    public class UserLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ICourseRepository _courseRepository;  // Thêm để check enroll
        private readonly ILogger<UserLessonController> _logger;

        public UserLessonController(ILessonService lessonService, ICourseRepository courseRepository, ILogger<UserLessonController> logger)
        {
            _lessonService = lessonService;
            _courseRepository = courseRepository;
            _logger = logger;
        }


        /// Lấy danh sách bài học theo CourseId (chỉ user đã enroll mới xem được)
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

                // Kiểm tra user đã enroll course chưa
                if (!await _courseRepository.IsUserEnrolled(courseId, userId))
                {
                    return Forbid("You are not enrolled in this course");
                }

                var response = await _lessonService.GetListLessonByCourseId(courseId);
                if (!response.Success) return BadRequest(response);
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
                var response = await _lessonService.GetLessonById(lessonId);
                if (!response.Success) return NotFound(response);  // Sửa: 404 nếu không tìm thấy
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