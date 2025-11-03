using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CleanDemo.API.Controller.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly IProgressService _progressService;

        public ProgressController(IProgressService progressService)
        {
            _progressService = progressService;
        }

        /// <summary>
        /// Cập nhật tiến độ học của một lesson
        /// </summary>
        /// <param name="lessonId">ID của lesson</param>
        /// <param name="completion">Phần trăm hoàn thành (0-100)</param>
        [HttpPost("lesson/{lessonId}/update")]
        public async Task<IActionResult> UpdateLessonProgress(int lessonId, [FromBody] UpdateProgressRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _progressService.UpdateLessonProgress(userId, lessonId, request.Completion);

            if (result.Success)
            {
                return Ok(new { message = result.Message, data = result.Data });
            }

            return BadRequest(new { message = result.Message });
        }

        /// <summary>
        /// Đánh dấu hoàn thành một lesson (100%)
        /// </summary>
        /// <param name="lessonId">ID của lesson</param>
        [HttpPost("lesson/{lessonId}/complete")]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _progressService.CompleteLessonAsync(userId, lessonId);

            if (result.Success)
            {
                return Ok(new { message = result.Message });
            }

            return BadRequest(new { message = result.Message });
        }

        /// <summary>
        /// Lấy tiến độ học của một course
        /// </summary>
        /// <param name="courseId">ID của course</param>
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseProgress(int courseId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _progressService.GetCourseProgressAsync(userId, courseId);

            if (result.Success)
            {
                return Ok(new { message = result.Message, data = result.Data });
            }

            return BadRequest(new { message = result.Message });
        }

        
        /// Lấy tiến độ học của tất cả courses
        
        [HttpGet("all")]
        public async Task<IActionResult> GetAllProgress()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();

            var result = await _progressService.GetAllUserProgressAsync(userId);

            if (result.Success)
            {
                return Ok(new { 
                    message = result.Message, 
                    data = result.Data,
                    total = result.Data?.Count ?? 0
                });
            }

            return BadRequest(new { message = result.Message });
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }

    public class UpdateProgressRequest
    {
        public double Completion { get; set; } // 0-100
    }
}
