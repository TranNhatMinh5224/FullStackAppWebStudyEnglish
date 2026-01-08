using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

// Admin Quản Lý người dùng trong khóa học 

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/courses")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminManageUserInCourseController : ControllerBase
    {
        private readonly IAdminCourseService _adminCourseService;
        private readonly IUserManagementService _userManagementService;
        private readonly IManageUserInCourseService _manageUserInCourseService;
        private readonly ILogger<AdminManageUserInCourseController> _logger;

        public AdminManageUserInCourseController(
            IAdminCourseService adminCourseService,
            IUserManagementService userManagementService,
            IManageUserInCourseService manageUserInCourseService,
            ILogger<AdminManageUserInCourseController> logger)
        {
            _adminCourseService = adminCourseService;
            _userManagementService = userManagementService;
            _manageUserInCourseService = manageUserInCourseService;
            _logger = logger;
        }
        // GET: Admin xem danh sách học viên trong khóa học (bất kỳ course nào)
        [HttpGet("{courseId}/students")]
        [RequirePermission("Admin.Course.Manage", "Admin.Course.Enroll")]
        public async Task<IActionResult> GetStudentsInCourse(int courseId, [FromQuery] PageRequest request)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} lấy danh sách students trong course {CourseId}", userId, courseId);

            var result = await _manageUserInCourseService.GetUsersByCourseIdPagedAsync(courseId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Admin xem chi tiết học viên trong khóa học
        [HttpGet("{courseId}/students/{studentId}")]
        [RequirePermission("Admin.Course.Manage", "Admin.Course.Enroll")]
        public async Task<IActionResult> GetStudentDetail(int courseId, int studentId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} xem chi tiết student {StudentId} trong course {CourseId}", 
                userId, studentId, courseId);

            var result = await _manageUserInCourseService.GetStudentDetailInCourseAsync(courseId, studentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: Admin thêm học viên vào khóa học (bằng email)
        [HttpPost("{courseId}/students")]
        [RequirePermission("Admin.Course.Manage", "Admin.Course.Enroll")]
        public async Task<IActionResult> AddStudentToCourse(int courseId, [FromBody] AddStudentToCourseDto request)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} thêm student {Email} vào course {CourseId}", 
                userId, request.Email, courseId);

            var result = await _manageUserInCourseService.AddStudentToCourseByEmailAsync(courseId, request.Email, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Admin xóa học viên khỏi khóa học
        [HttpDelete("{courseId}/students/{studentId}")]
        [RequirePermission("Admin.Course.Manage", "Admin.Course.Enroll")]
        public async Task<IActionResult> RemoveStudentFromCourse(int courseId, int studentId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} xóa student {StudentId} khỏi course {CourseId}", 
                userId, studentId, courseId);

            var result = await _manageUserInCourseService.RemoveStudentFromCourseAsync(courseId, studentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
