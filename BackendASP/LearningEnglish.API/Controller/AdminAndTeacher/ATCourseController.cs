using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/courses")]
    [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
    public class CourseController : BaseController
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

        // GET: api/courses/course-types - lấy danh sách loại khoá học (System, Teacher)
        [HttpGet("course-types")]
        [Authorize(Roles = "SuperAdmin,Admin,Teacher")]
        public IActionResult GetCourseTypes()
        {
            var courseTypes = _adminCourseService.GetCourseTypes();
            return Ok(new { success = true, data = courseTypes });
        }

        // GET: api/courses - admin lấy tất cả khoá học với phân trang, filter, sort, search
        // Query params: 
        // - pageNumber, pageSize: phân trang
        // - type: lọc theo loại (1=System, 2=Teacher)
        // - sortBy: title (mặc định), createdat, price, enrollmentcount, updatedat
        // - sortOrder: 1=A-Z (Ascending), 2=Z-A (Descending)
        // - searchTerm: tìm kiếm theo tên khóa học, mã khóa học, tên giáo viên
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetAllCourses([FromQuery] CourseQueryParameters parameters)
        {
            var pagedResult = await _adminCourseService.GetAllCoursesPagedAsync(parameters);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // DELETE: api/courses/{courseId} - admin xoá khoá học
        [HttpDelete("{courseId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var result = await _adminCourseService.DeleteCourseAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses - admin tao khoá học mới
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AdminCreateCourse([FromBody] AdminCreateCourseRequestDto requestDto)
        {

            var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);
            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/courses/teacher - giáo viên tạo khoá học mới
        [HttpPost("teacher")]
        [Authorize(Roles = "SuperAdmin,Teacher")]
        public async Task<IActionResult> CreateCourse([FromBody] TeacherCreateCourseRequestDto requestDto)
        {

            var teacherId = GetCurrentUserId();
            var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);
            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/courses/teacher - giáo viên lấy khoá học của mình với phân trang, sort, search
        // Query params:
        // - pageNumber, pageSize: phân trang
        // - sortBy: title (mặc định), createdat, updatedat
        // - sortOrder: 1=A-Z (Ascending), 2=Z-A (Descending)
        // - searchTerm: tìm kiếm theo tên khóa học, mã khóa học
        [HttpGet("teacher")]
        [Authorize(Roles = "SuperAdmin,Teacher")]
        public async Task<IActionResult> GetMyCourses([FromQuery] CourseQueryParameters parameters)
        {
            var teacherId = GetCurrentUserId();
            var pagedResult = await _teacherCourseService.GetMyCoursesPagedAsync(teacherId, parameters);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/courses/teacher/{courseId} - giáo viên lấy chi tiết khoá học của mình
        [HttpGet("teacher/{courseId}")]
        [Authorize(Roles = "SuperAdmin,Teacher")]
        public async Task<IActionResult> GetCourseDetail(int courseId)
        {
            var teacherId = GetCurrentUserId();
            var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/courses/{courseId} - admin sửa khoá học
        [HttpPut("{courseId}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> AdminUpdateCourse(int courseId, [FromBody] AdminUpdateCourseRequestDto requestDto)
        {

            var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/courses/teacher/{courseId} - giáo viên sửa khoá học của mình
        [HttpPut("teacher/{courseId}")]
        [Authorize(Roles = "SuperAdmin,Teacher")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherUpdateCourseRequestDto requestDto)
        {

            var teacherId = GetCurrentUserId();
            var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/courses/{courseId}/students - lấy danh sách học viên trong khoá học với phân trang và tìm kiếm
        // Query parameters:
        // - PageNumber, PageSize: Phân trang
        // - SearchTerm: Tìm kiếm theo DisplayName (FirstName + LastName) và Email
        [HttpGet("{courseId}/students")]
        [Authorize(Roles = "Admin, Teacher")]
        public async Task<IActionResult> GetUsersByCourseId(int courseId, [FromQuery] PageRequest request)
        {
            var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();
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
            var userId = GetCurrentUserId();
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

