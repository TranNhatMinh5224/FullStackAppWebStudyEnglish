using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;


// Admin Crud khóa học

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/courses")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminCourseController : ControllerBase
    {
        private readonly IAdminCourseService _adminCourseService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<AdminCourseController> _logger;

        public AdminCourseController(
            IAdminCourseService adminCourseService,
            IUserManagementService userManagementService,
            ILogger<AdminCourseController> logger)
        {
            _adminCourseService = adminCourseService;
            _userManagementService = userManagementService;
            _logger = logger;
        }

        // GET: Admin lấy danh sách tất cả khóa học 
        [HttpGet]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> GetAllCourses([FromQuery] AdminCourseQueryParameters request)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} lấy danh sách courses", userId);

            var result = await _adminCourseService.GetAllCoursesPagedAsync(request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: Admin tạo khóa học mới
        [HttpPost]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> CreateCourse([FromBody] AdminCreateCourseRequestDto requestDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} tạo course: {Title}", userId, requestDto.Title);

            var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);
            return result.Success
                ? CreatedAtAction(nameof(GetAllCourses), new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: Admin cập nhật khóa học
        [HttpPut("{courseId}")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] AdminUpdateCourseRequestDto requestDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} cập nhật course {CourseId}", userId, courseId);

            var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Admin xóa khóa học
        [HttpDelete("{courseId}")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} xóa course {CourseId}", userId, courseId);

            var result = await _adminCourseService.DeleteCourseAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
