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
        private readonly IUserManagementService _userManagementService;

        public CourseController(
            IAdminCourseService adminCourseService,
            ITeacherCourseService teacherCourseService,
            IUserEnrollmentService userEnrollmentService,
            ILogger<CourseController> logger,
            IUserManagementService userManagementService)
        {
            _adminCourseService = adminCourseService;
            _teacherCourseService = teacherCourseService;
            _userEnrollmentService = userEnrollmentService;
            _logger = logger;
            _userManagementService = userManagementService;
        }


        // Admin - Lấy tất cả khóa học

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


        // Controller Admin - Xóa khóa học

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


        // Controller Admin - Tạo khóa học mới

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




        // Controller Teacher - Tạo khóa học mới

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


        // Controller Teacher - Lấy danh sách khóa học của mình

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


        // Controller Teacher - Tham gia khóa học của teacher khác

        // [HttpPost("teacher/join-course")]
        // [Authorize(Roles = "Teacher")]
        // public async Task<IActionResult> JoinCourse([FromBody] JoinCourseTeacherDto joinDto)
        // {
        //     try
        //     {
        //         if (!ModelState.IsValid)
        //         {
        //             return BadRequest(ModelState);
        //         }

        //         // Lấy TeacherId từ JWT token
        //         var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //         if (!int.TryParse(teacherIdClaim, out int teacherId))
        //         {
        //             return Unauthorized(new { message = "Invalid teacher credentials" });
        //         }

        //         // Convert JoinCourseTeacherDto to EnrollCourseDto
        //         var enrollDto = new EnrollCourseDto { CourseId = joinDto.CourseId };
        //         var result = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, teacherId);

        //         if (!result.Success)
        //         {
        //             return BadRequest(result);
        //         }

        //         return Ok(result);
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error in JoinCourse endpoint");
        //         return StatusCode(500, new { message = "Internal server error" });
        //     }
        // }


        // Controller Admin - Cập nhật khóa học

        [HttpPut("admin/{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUpdateCourse(int courseId, [FromBody] AdminUpdateCourseRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
                }

                var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);

                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message,
                        statusCode = result.StatusCode
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data,
                    statusCode = result.StatusCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AdminUpdateCourse endpoint for CourseId: {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống", statusCode = 500 });
            }
        }



        // Controller Teacher - Cập nhật khóa học của mình

        [HttpPut("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherUpdateCourseRequestDto requestDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
                }

                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized(new { success = false, message = "Thông tin giáo viên không hợp lệ" });
                }

                var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);

                if (!result.Success)
                {
                    return StatusCode(result.StatusCode, new
                    {
                        success = false,
                        message = result.Message,
                        statusCode = result.StatusCode
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = result.Message,
                    data = result.Data,
                    statusCode = result.StatusCode
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCourse endpoint for CourseId: {CourseId}", courseId);
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống", statusCode = 500 });
            }
        }
        // Controller lấy danh sách User trong khóa học (theo courseId) dành cho Admin và Teacher
        [HttpGet("getusersbycourse/{courseId}")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> GetUsersByCourseId(int courseId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var checkRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }
            if (string.IsNullOrEmpty(checkRole))
            {
                return Unauthorized(new { message = "User role not found" });
            }
            var result = await _userManagementService.GetUsersByCourseIdAsync(courseId, userId, checkRole);
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }
            return Ok(result.Data);
        }
    }
}

