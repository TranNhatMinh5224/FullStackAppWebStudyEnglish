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

       
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetLessonsByCourseId(int courseId)
        {
          
            var userIdValue = User.GetUserIdSafe();
            int? userId = userIdValue > 0 ? userIdValue : null;

            var result = await _lessonService.GetListLessonByCourseId(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint User lấy chi tiết bài học
     
        [HttpGet("{lessonId}")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
         
            var result = await _lessonService.GetLessonById(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
