using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;

namespace CleanDemo.API.Controller.User
{
    [ApiController]
    [Route("api/user/courses")]
    public class UserCourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<UserCourseController> _logger;

        public UserCourseController(ICourseService courseService, ILogger<UserCourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách khóa học hệ thống (không cần đăng nhập)
        /// </summary>
        [HttpGet("system-courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemCourses()
        {
            try
            {
                // Lấy UserId nếu user đã đăng nhập (optional)
                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var result = await _courseService.GetSystemCoursesAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSystemCourses endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy chi tiết khóa học (không cần đăng nhập)
        /// </summary>
        [HttpGet("{courseId}/details")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseDetail(int courseId)
        {
            try
            {
                // Lấy UserId nếu user đã đăng nhập (optional)
                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var result = await _courseService.GetCourseDetailAsync(courseId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCourseDetail endpoint for CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Đăng ký khóa học (yêu cầu đăng nhập)
        /// </summary>
        [HttpPost("enroll")]
        [Authorize(Roles = "Student,User")]
        public async Task<IActionResult> EnrollInCourse([FromBody] EnrollCourseDto enrollDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Lấy UserId từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var result = await _courseService.EnrollInCourseAsync(enrollDto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EnrollInCourse endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Hủy đăng ký khóa học
        /// </summary>
        [HttpDelete("unenroll/{courseId}")]
        [Authorize(Roles = "Student,User")]
        public async Task<IActionResult> UnenrollFromCourse(int courseId)
        {
            try
            {
                // Lấy UserId từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var result = await _courseService.UnenrollFromCourseAsync(courseId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UnenrollFromCourse endpoint for CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Lấy danh sách khóa học đã đăng ký
        /// </summary>
        [HttpGet("my-enrolled-courses")]
        [Authorize(Roles = "Student,User,Teacher")]
        public async Task<IActionResult> GetMyEnrolledCourses()
        {
            try
            {
                // Lấy UserId từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var result = await _courseService.GetMyEnrolledCoursesAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMyEnrolledCourses endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}