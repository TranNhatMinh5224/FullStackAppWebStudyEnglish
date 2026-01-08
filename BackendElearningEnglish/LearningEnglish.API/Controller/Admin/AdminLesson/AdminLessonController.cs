using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lesson;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

// Admin quản lý bài học

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/lessons")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminLessonController : ControllerBase
    {
        private readonly IAdminLessonService _lessonService;
        private readonly ILogger<AdminLessonController> _logger;

        public AdminLessonController(IAdminLessonService lessonService, ILogger<AdminLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        // POST: Admin tạo bài học mới (System course only)
        [HttpPost]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> AddLesson(AdminCreateLessonDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo lesson cho course {CourseId}", adminId, dto.CourseId);

            var result = await _lessonService.AdminAddLesson(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Admin xem chi tiết bài học
        [HttpGet("{lessonId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem lesson {LessonId}", adminId, lessonId);

            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Admin lấy danh sách bài học theo khóa học
        [HttpGet("course/{courseId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách lessons của course {CourseId}", adminId, courseId);

            var result = await _lessonService.GetListLessonByCourseId(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: Admin cập nhật bài học
        [HttpPut("{lessonId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật lesson {LessonId}", adminId, lessonId);

            var result = await _lessonService.UpdateLesson(lessonId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Admin xóa bài học
        [HttpDelete("{lessonId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa lesson {LessonId}", adminId, lessonId);

            var result = await _lessonService.DeleteLesson(lessonId);
            return result.Success ? NoContent() : StatusCode(result.StatusCode, result);
        }
    }
}

