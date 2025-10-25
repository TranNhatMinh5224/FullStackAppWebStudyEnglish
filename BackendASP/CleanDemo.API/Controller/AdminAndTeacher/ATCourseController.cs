using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;

namespace CleanDemo.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/")]
    [Authorize(Roles = "Admin, Teacher")]
    public class CourseController : ControllerBase
    {
        private readonly IAdminCourseService _adminCourseService;
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            IAdminCourseService adminCourseService,
            ITeacherCourseService teacherCourseService,
            IUserEnrollmentService userEnrollmentService,
            ILogger<CourseController> logger)
        {
            _adminCourseService = adminCourseService;
            _teacherCourseService = teacherCourseService;
            _userEnrollmentService = userEnrollmentService;
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
                var result = await _adminCourseService.GetAllCoursesAsync();

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
                var result = await _adminCourseService.DeleteCourseAsync(courseId);

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

                var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result);
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

                var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result);
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

                var result = await _teacherCourseService.GetMyCoursesByTeacherAsync(teacherId);

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

                // Convert JoinCourseTeacherDto to EnrollCourseDto
                var enrollDto = new EnrollCourseDto { CourseId = joinDto.CourseId };
                var result = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, teacherId);

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

        // === SHARED ENDPOINTS (Admin & Teacher) ===
    }
}

