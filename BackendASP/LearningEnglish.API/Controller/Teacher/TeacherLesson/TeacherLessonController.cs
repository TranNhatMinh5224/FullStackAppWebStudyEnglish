using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.Teacher
{
    [ApiController]
    [Route("api/teacher/lessons")]
    [RequireTeacherRole]
    public class TeacherLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<TeacherLessonController> _logger;

        public TeacherLessonController(ILessonService lessonService, ILogger<TeacherLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        // POST: api/teacher/lessons - Teacher tạo bài học (Teacher course)
        [HttpPost]
        public async Task<IActionResult> AddLessonForTeacher(TeacherCreateLessonDto dto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo lesson cho course {CourseId}", teacherId, dto.CourseId);

            var result = await _lessonService.TeacherAddLesson(dto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lessons/{lessonId} - Teacher xem chi tiết bài học
        // RLS: lessons_policy_teacher_all_own (Teacher chỉ xem lessons của own courses)
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem lesson {LessonId}", teacherId, lessonId);

            // RLS đã filter: Teacher chỉ xem được lessons của own courses
            // Nếu lesson không thuộc own course → RLS sẽ filter → lesson == null → service trả về 404
            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/lessons/course/{courseId} - Teacher xem danh sách lessons theo course
        // RLS: lessons_policy_teacher_all_own (Teacher chỉ xem lessons của own courses)
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách lessons của course {CourseId}", teacherId, courseId);

            // RLS đã filter: Teacher chỉ xem được lessons của own courses
            // Nếu course không thuộc về teacher → RLS sẽ filter → lessons = empty list
            // userId = null vì Teacher không cần progress info
            var result = await _lessonService.GetListLessonByCourseId(courseId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/lessons/{lessonId} - Teacher cập nhật bài học
        // RLS: lessons_policy_teacher_all_own (Teacher chỉ cập nhật lessons của own courses)
        [HttpPut("{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật lesson {LessonId}", teacherId, lessonId);

            // RLS đã filter: Teacher chỉ cập nhật được lessons của own courses
            // Nếu lesson không thuộc own course → RLS sẽ filter → lesson == null → service trả về 404
            var result = await _lessonService.UpdateLesson(lessonId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/lessons/{lessonId} - Teacher xóa bài học
        // RLS: lessons_policy_teacher_all_own (Teacher chỉ xóa lessons của own courses)
        [HttpDelete("{lessonId}")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa lesson {LessonId}", teacherId, lessonId);

            // RLS đã filter: Teacher chỉ xóa được lessons của own courses
            // Nếu lesson không thuộc own course → RLS sẽ filter → lesson == null → service trả về 404
            var result = await _lessonService.DeleteLesson(lessonId);
            return result.Success ? NoContent() : StatusCode(result.StatusCode, result);
        }
    }
}

