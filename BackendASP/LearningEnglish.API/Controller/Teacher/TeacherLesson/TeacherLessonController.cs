using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.ILesson;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

// Teacher quản lý bài học

namespace LearningEnglish.API.Controller.Teacher
{
    [ApiController]
    [Route("api/teacher/lessons")]
    [RequireTeacherRole]
    public class TeacherLessonController : ControllerBase
    {
        private readonly ITeacherLessonService _lessonService;
        private readonly ILogger<TeacherLessonController> _logger;

        public TeacherLessonController(ITeacherLessonService lessonService, ILogger<TeacherLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        // POST: Teacher tạo bài học mới (own course only)
        [HttpPost]
        public async Task<IActionResult> AddLessonForTeacher(TeacherCreateLessonDto dto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo lesson cho course {CourseId}", teacherId, dto.CourseId);

            var result = await _lessonService.TeacherAddLesson(dto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Teacher xem chi tiết bài học (own course only)
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem lesson {LessonId}", teacherId, lessonId);

            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Teacher lấy danh sách bài học theo khóa học (own course only)
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách lessons của course {CourseId}", teacherId, courseId);

            var result = await _lessonService.GetListLessonByCourseId(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: Teacher cập nhật bài học (own course only)
        [HttpPut("{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật lesson {LessonId}", teacherId, lessonId);

            var result = await _lessonService.UpdateLesson(lessonId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Teacher xóa bài học (own course only)
        [HttpDelete("{lessonId}")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa lesson {LessonId}", teacherId, lessonId);

            var result = await _lessonService.DeleteLesson(lessonId);
            return result.Success ? NoContent() : StatusCode(result.StatusCode, result);
        }
    }
}

