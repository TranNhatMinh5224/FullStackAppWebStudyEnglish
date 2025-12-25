using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/courses")]
    [Authorize(Roles = "SuperAdmin, Admin, Teacher")]
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

        // GET: api/courses - Admin lấy tất cả khoá học với phân trang
        [HttpGet]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> GetAllCourses([FromQuery] CourseQueryParameters request)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("🔐 Admin {UserId} đang lấy danh sách courses", userId);
            
            // PageRequest có giá trị mặc định (PageNumber=1, PageSize=20), luôn dùng phân trang
            var pagedResult = await _adminCourseService.GetAllCoursesPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // DELETE: api/courses/{courseId} - Admin xoá khoá học
        [HttpDelete("{courseId}")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("🔐 Admin {UserId} đang xóa course {CourseId}", userId, courseId);

            var result = await _adminCourseService.DeleteCourseAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses - Admin tạo khoá học mới
        [HttpPost("create/admin")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> AdminCreateCourse([FromBody] AdminCreateCourseRequestDto requestDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation(
                "🔐 Admin {UserId} đang tạo course mới: {Title}",
                userId, requestDto.Title);

            var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);
            
            if (result.Success)
            {
                _logger.LogInformation(
                    " Admin {UserId} tạo course thành công: CourseId={CourseId}, Title={Title}",
                    userId, result.Data?.CourseId, requestDto.Title);
            }
            else
            {
                _logger.LogWarning(
                    "admin {UserId} tạo course thất bại: {Message}",
                    userId, result.Message);
            }

            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses/teacher - giáo viên tạo khoá học mới
        [HttpPost("create/teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateCourse([FromBody] TeacherCreateCourseRequestDto requestDto)
        {

            var teacherId = User.GetUserId();
            var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);
            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/courses/teacher - giáo viên lấy tất cả khoá học của mình với phân trang
        [HttpGet("teacher")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyCourses([FromQuery] CourseQueryParameters request)
        {
            var teacherId = User.GetUserId();

            // CourseQueryParameters có giá trị mặc định, luôn dùng phân trang
            var pagedResult = await _teacherCourseService.GetMyCoursesPagedAsync(teacherId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/courses/teacher/{courseId} - giáo viên lấy chi tiết khoá học của mình
        [HttpGet("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetCourseDetail(int courseId)
        {
            var teacherId = User.GetUserId();
            var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/courses/{courseId} - Admin sửa khoá học
        [HttpPut("{courseId}")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> AdminUpdateCourse(int courseId, [FromBody] AdminUpdateCourseRequestDto requestDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("🔐 Admin {UserId} đang sửa course {CourseId}", userId, courseId);

            var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/courses/teacher/{courseId} - giáo viên sửa khoá học của mình
        [HttpPut("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherUpdateCourseRequestDto requestDto)
        {

            var teacherId = User.GetUserId();
            var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/courses/{courseId}/students - lấy danh sách học viên trong khoá học với phân trang
        [HttpGet("{courseId}/students")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> GetUsersByCourseId(int courseId, [FromQuery] PageRequest request)
        {
            var userId = User.GetUserId();
            var checkRole = User.GetPrimaryRole();

            if (string.IsNullOrEmpty(checkRole))
            {
                return Unauthorized(new { message = "User role not found" });
            }

            // PageRequest có giá trị mặc định, luôn dùng phân trang
            var pagedResult = await _userManagementService.GetUsersByCourseIdPagedAsync(courseId, userId, checkRole, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/courses/{courseId}/students/{studentId} - lấy chi tiết học viên trong khoá học
        // RLS tự động filter: Admin xem tất cả, Teacher chỉ xem students trong own courses
        [HttpGet("{courseId}/students/{studentId}")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> GetStudentDetailInCourse(int courseId, int studentId)
        {
            var userId = User.GetUserId();
            var role = User.GetPrimaryRole();

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "User role not found" });
            }

            var result = await _userManagementService.GetStudentDetailInCourseAsync(courseId, studentId, userId, role);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses/{courseId}/students - Mời học viên vào khoá học qua email
        // Admin: thêm vào bất kỳ course nào, Teacher: chỉ thêm vào own courses (RLS filter)
        [HttpPost("{courseId}/students")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> AddStudentToCourse(int courseId, [FromBody] AddStudentToCourseDto request)
        {
            var userId = User.GetUserId();
            var role = User.GetPrimaryRole();

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "User role not found" });
            }

            var result = await _userManagementService.AddStudentToCourseByEmailAsync(courseId, request.Email, userId, role);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/courses/{courseId}/students/{studentId} - xoá học viên khỏi khoá học
        // Admin: xóa bất kỳ student nào, Teacher: chỉ xóa students trong own courses (RLS filter)
        [HttpDelete("{courseId}/students/{studentId}")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> RemoveStudentFromCourse(int courseId, int studentId)
        {
            var userId = User.GetUserId();
            var role = User.GetPrimaryRole();

            if (string.IsNullOrEmpty(role))
            {
                return Unauthorized(new { message = "User role not found" });
            }

            var result = await _userManagementService.RemoveStudentFromCourseAsync(courseId, studentId, userId, role);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

