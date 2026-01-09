using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Lesson;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;

// User xem bài học và tiến độ học tập

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

        // GET: User xem chi tiết bài học + progress
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang xem lesson {LessonId}", userId, lessonId);

            var result = await _lessonService.GetLessonById(lessonId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: User lấy danh sách bài học theo khóa học + progress
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang xem danh sách lessons của course {CourseId}", userId, courseId);

            var result = await _lessonService.GetListLessonByCourseId(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
