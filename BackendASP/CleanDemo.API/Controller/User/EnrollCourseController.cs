using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace CleanDemo.API.Controller.User
{
    [ApiController]
    [Route("api/user/enroll")]
    [Authorize(Roles = "Student,User")]
    public class EnrollCourseController : ControllerBase
    {
        private readonly IEnrollCourseService _enrollCourseService;
        private readonly ILogger<EnrollCourseController> _logger;

        public EnrollCourseController(IEnrollCourseService enrollCourseService, ILogger<EnrollCourseController> logger)
        {
            _enrollCourseService = enrollCourseService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng ký khóa học
        /// </summary>
        [HttpPost("course")]
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

                var result = await _enrollCourseService.EnrollInCourseAsync(enrollDto, userId);

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
        [HttpDelete("course/{courseId}")]
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

                var result = await _enrollCourseService.UnenrollFromCourseAsync(courseId, userId);

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
        [HttpGet("my-courses")]
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

                var result = await _enrollCourseService.GetMyEnrolledCoursesAsync(userId);

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
