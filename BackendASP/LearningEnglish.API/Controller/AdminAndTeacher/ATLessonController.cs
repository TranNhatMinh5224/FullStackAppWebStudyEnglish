using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/lessons")]
    [Authorize]
    public class ATLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<ATLessonController> _logger;

        public ATLessonController(ILessonService lessonService, ILogger<ATLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        // endpoint Admin tạo bài học
        [HttpPost("admin/add")]
        [RequirePermission("Admin.Lesson.Manage")]
        public async Task<IActionResult> AddLesson(AdminCreateLessonDto dto)
        {
            var result = await _lessonService.AdminAddLesson(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher tạo bài học
        [HttpPost("teacher/add")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddLessonForTeacher(TeacherCreateLessonDto dto)
        {
            // [Authorize(Roles = "Teacher")] đảm bảo userId luôn có
            var userId = User.GetUserId();
            var result = await _lessonService.TeacherAddLesson(dto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin/Teacher xóa bài học (RLS đã filter theo ownership)
        [HttpDelete("delete/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            // RLS đã filter, không cần truyền userId/userRole vào service
            // Nếu lesson không tồn tại hoặc không thuộc về teacher → RLS sẽ filter → service trả về null → 404
            var result = await _lessonService.DeleteLesson(lessonId);
            return result.Success ? NoContent() : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin/Teacher cập nhật bài học (RLS đã filter theo ownership)
        [HttpPut("update/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            // RLS đã filter, không cần truyền userId/userRole vào service
            var result = await _lessonService.UpdateLesson(lessonId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin/Teacher lấy chi tiết bài học (RLS đã filter theo ownership)
        [HttpGet("get/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            // RLS đã filter, không cần truyền userId/userRole vào service
            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin/Teacher/Student lấy danh sách bài học theo course (RLS đã filter)
        [HttpGet("course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            // RLS đã filter theo course ownership/enrollment
            // userId optional: chỉ cần khi tính progress cho Student
            var userIdValue = User.GetUserIdSafe();
            int? userId = userIdValue > 0 ? userIdValue : null;
            
            var result = await _lessonService.GetListLessonByCourseId(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
