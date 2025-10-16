using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;

namespace CleanDemo.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/")]
    [Authorize]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(ICourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
        }

        // === ADMIN ENDPOINTS ===

        /// <summary>
        /// Admin - Lấy tất cả khóa học
        /// </summary>
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCourses()
        {
            try
            {
                var result = await _courseService.GetAllCoursesAsync();

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllCourses endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Admin - Xóa khóa học
        /// </summary>
        [HttpDelete("admin/{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            try
            {
                var result = await _courseService.DeleteCourseAsync(courseId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCourse endpoint for CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Admin - Tạo khóa học mới
        /// </summary>
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreateCourse([FromBody] AdminCreateCourseRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _courseService.AdminCreateCourseAsync(requestDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetCourseDetail), new { courseId = result.Data?.CourseId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AdminCreateCourse endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // === TEACHER ENDPOINTS ===

        /// <summary>
        /// Teacher - Tạo khóa học mới
        /// </summary>
        [HttpPost("teacher/create")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateCourse([FromBody] TeacherCreateCourseRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Lấy TeacherId từ JWT token
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized(new { message = "Invalid teacher credentials" });
                }

                var result = await _courseService.CreateCourseAsync(requestDto, teacherId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetCourseDetail), new { courseId = result.Data?.CourseId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateCourse endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Teacher - Lấy danh sách khóa học của mình
        /// </summary>
        [HttpGet("teacher/my-courses")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyCourses()
        {
            try
            {
                // Lấy TeacherId từ JWT token
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized(new { message = "Invalid teacher credentials" });
                }

                var result = await _courseService.GetMyCoursesByTeacherAsync(teacherId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetMyCourses endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Teacher - Tham gia khóa học của teacher khác
        /// </summary>
        [HttpPost("teacher/join-course")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> JoinCourse([FromBody] JoinCourseTeacherDto joinDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Lấy TeacherId từ JWT token
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized(new { message = "Invalid teacher credentials" });
                }

                var result = await _courseService.JoinCourseAsTeacherAsync(joinDto, teacherId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in JoinCourse endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Teacher - Cập nhật khóa học
        /// </summary>
        [HttpPut("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherCreateCourseRequestDto courseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Lấy TeacherId từ JWT token để kiểm tra quyền sở hữu
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized(new { message = "Invalid teacher credentials" });
                }

                // Kiểm tra teacher có quyền cập nhật khóa học này không
                var courseDetail = await _courseService.GetCourseDetailAsync(courseId);
                if (!courseDetail.Success || courseDetail.Data?.TeacherId != teacherId)
                {
                    return Forbid("You don't have permission to update this course");
                }

                var result = await _courseService.UpdateCourseAsync(courseId, courseDto, teacherId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCourse endpoint for CourseId: {CourseId}", courseId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // === SHARED ENDPOINTS (Admin & Teacher) ===

        /// <summary>
        /// Admin & Teacher - Lấy chi tiết khóa học
        /// </summary>
        [HttpGet("{courseId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetCourseDetail(int courseId)
        {
            try
            {
                // Lấy UserId từ JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                int? userId = int.TryParse(userIdClaim, out int parsedUserId) ? parsedUserId : null;

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
    }
}

