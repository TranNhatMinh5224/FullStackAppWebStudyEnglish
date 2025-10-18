using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;


namespace CleanDemo.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/lesson/")]
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

        [HttpPost("admin/add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLesson(AdminCreateLessonDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _lessonService.AdminAddLesson(dto);
                if (!response.Success) return BadRequest(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson for admin");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("teacher/add")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddLessonForTeacher(TeacherCreateLessonDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _lessonService.TeacherAddLesson(dto);
                if (!response.Success) return BadRequest(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson for teacher");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete/{lessonId}")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            try
            {
                var response = await _lessonService.DeleteLesson(new DeleteLessonDto { LessonId = lessonId });
                if (!response.Success) return BadRequest(response);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson {LessonId}", lessonId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("update/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                dto.LessonId = lessonId;  // Gán ID từ URL
                var response = await _lessonService.UpdateLesson(dto);
                if (!response.Success) return BadRequest(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson {LessonId}", lessonId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("get/{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            try
            {
                var response = await _lessonService.GetLessonById(lessonId);
                if (!response.Success) return NotFound(response);  // 404 nếu không tìm thấy
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson {LessonId}", lessonId);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            try
            {
                var response = await _lessonService.GetListLessonByCourseId(courseId);
                if (!response.Success) return BadRequest(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons for course {CourseId}", courseId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}