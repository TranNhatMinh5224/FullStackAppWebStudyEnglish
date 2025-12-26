using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/lessons")]
    [Authorize]
    public class UserLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<UserLessonController> _logger;

        public UserLessonController(ILessonService lessonService, ILogger<UserLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        // endpoint User lấy danh sách bài học theo course (RLS đã filter, userId để tính progress)
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
            // RLS đã filter theo enrollment/ownership
            // userId cần để tính progress cho Student
            var userIdValue = User.GetUserIdSafe();
            int? userId = userIdValue > 0 ? userIdValue : null;

            var result = await _lessonService.GetListLessonByCourseId(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint User lấy chi tiết bài học (RLS đã filter)
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            // RLS đã filter theo enrollment/ownership
            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
