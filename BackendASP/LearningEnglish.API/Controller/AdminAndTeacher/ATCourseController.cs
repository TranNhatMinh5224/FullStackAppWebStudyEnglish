using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/courses")]
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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user credentials");
            }
            return userId;
        }

        // GET: api/courses?scope=all - Admin retrieves all courses
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllCourses([FromQuery] PageRequest? request)
        {
            // Nếu có parameters phân trang, gọi service phân trang
            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _adminCourseService.GetAllCoursesPagedAsync(request);
                return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
            }

            // Không có parameters, trả về tất cả
            var result = await _adminCourseService.GetAllCoursesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/courses/{courseId} - Admin deletes a course by ID
        [HttpDelete("{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var result = await _adminCourseService.DeleteCourseAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses - Admin creates a new course with system-level permissions
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminCreateCourse([FromBody] AdminCreateCourseRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);
            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses/teacher - Teacher creates a new course owned by their account
        [HttpPost("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateCourse([FromBody] TeacherCreateCourseRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacherId = GetCurrentUserId();
            var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);
            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/courses/teacher - Teacher retrieves their courses
        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyCourses([FromQuery] PageRequest? request)
        {
            var teacherId = GetCurrentUserId();

            // Nếu có parameters phân trang, gọi service phân trang
            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _teacherCourseService.GetMyCoursesPagedAsync(teacherId, request);
                return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
            }

            // Không có parameters, trả về tất cả
            var result = await _teacherCourseService.GetMyCoursesByTeacherAsync(teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/courses/{courseId} - Admin updates any course with full permissions
        [HttpPut("{courseId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUpdateCourse(int courseId, [FromBody] AdminUpdateCourseRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/courses/teacher/{courseId} - Teacher updates their own course
        [HttpPut("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherUpdateCourseRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });
            }

            var teacherId = GetCurrentUserId();
            var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/courses/{courseId}/students - Get students in course
        [HttpGet("{courseId}/students")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> GetUsersByCourseId(int courseId, [FromQuery] PageRequest? request)
        {
            var userId = GetCurrentUserId();
            var checkRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(checkRole))
            {
                return Unauthorized(new { message = "User role not found" });
            }

            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _userManagementService.GetUsersByCourseIdPagedAsync(courseId, userId, checkRole, request);
                return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
            }

            var result = await _userManagementService.GetUsersByCourseIdAsync(courseId, userId, checkRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

