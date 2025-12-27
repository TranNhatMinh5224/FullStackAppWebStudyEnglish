using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/lessons")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<AdminLessonController> _logger;

        public AdminLessonController(ILessonService lessonService, ILogger<AdminLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        // POST: api/admin/lessons - Admin tạo bài học (System course)
        [HttpPost]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> AddLesson(AdminCreateLessonDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo lesson cho course {CourseId}", adminId, dto.CourseId);

            var result = await _lessonService.AdminAddLesson(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lessons/{lessonId} - Admin xem chi tiết bài học
        // RLS: lessons_policy_admin_all (Admin có quyền xem tất cả lessons)
        [HttpGet("{lessonId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem lesson {LessonId}", adminId, lessonId);

            // RLS đã filter: Admin có quyền xem tất cả lessons
            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/lessons/course/{courseId} - Admin xem danh sách lessons theo course
        // RLS: lessons_policy_admin_all (Admin có quyền xem tất cả lessons)
        [HttpGet("course/{courseId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách lessons của course {CourseId}", adminId, courseId);

            // RLS đã filter: Admin có quyền xem tất cả lessons
            // userId = null vì Admin không cần progress info
            var result = await _lessonService.GetListLessonByCourseId(courseId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/lessons/{lessonId} - Admin cập nhật bài học
        // RLS: lessons_policy_admin_all (Admin có quyền cập nhật tất cả lessons)
        [HttpPut("{lessonId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật lesson {LessonId}", adminId, lessonId);

            // RLS đã filter: Admin có quyền cập nhật tất cả lessons
            var result = await _lessonService.UpdateLesson(lessonId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/lessons/{lessonId} - Admin xóa bài học
        // RLS: lessons_policy_admin_all (Admin có quyền xóa tất cả lessons)
        [HttpDelete("{lessonId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa lesson {LessonId}", adminId, lessonId);

            // RLS đã filter: Admin có quyền xóa tất cả lessons
            var result = await _lessonService.DeleteLesson(lessonId);
            return result.Success ? NoContent() : StatusCode(result.StatusCode, result);
        }
    }
}

