using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;

namespace CleanDemo.API.Controller.User
{
    [ApiController]
    [Route("api/user/lessons/")]  // Thêm route base
    [AllowAnonymous]  // Giữ nếu public
    public class UserLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<UserLessonController> _logger;

        public UserLessonController(ILessonService lessonService, ILogger<UserLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }


        /// Lấy danh sách bài học theo CourseId 
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
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

        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            try
            {
                var response = await _lessonService.GetLessonById(lessonId);
                if (!response.Success) return NotFound(response);  // Sửa: 404 nếu không tìm thấy
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson {LessonId}", lessonId);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}