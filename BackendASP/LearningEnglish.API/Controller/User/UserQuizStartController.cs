using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/User/QuizAttempt")]
    [Authorize(Roles = "Student")]
    public class UserQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _quizAttemptService;

        public UserQuizAttemptController(IQuizAttemptService quizAttemptService)
        {
            _quizAttemptService = quizAttemptService;
        }

        // Bắt đầu làm quiz
        [HttpPost("start/{quizId}")]
        public async Task<IActionResult> StartQuizAttempt(int quizId)
        {
            // Lấy userId từ token
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user ID" });
            }

            var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Cập nhật điểm cho câu hỏi (real-time)
        [HttpPost("update-score/{quizId}")]
        public async Task<IActionResult> UpdateScore(int quizId, [FromBody] UpdateScoreRequestDto request)
        {
            var result = await _quizAttemptService.UpdateScoreAsync(quizId, request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Submit bài thi
        [HttpPost("submit/{attemptId}")]
        public async Task<IActionResult> SubmitQuizAttempt(int attemptId)
        {
            var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Resume attempt (cho trường hợp disconnect rồi quay lại)
        [HttpGet("resume/{attemptId}")]
        public async Task<IActionResult> ResumeQuizAttempt(int attemptId)
        {
            var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Update câu trả lời và tính điểm ngay lập tức
        [HttpPost("update-answer/{attemptId}")]
        public async Task<IActionResult> UpdateAnswerAndScore(int attemptId, [FromBody] UpdateAnswerRequestDto request)
        {
            var result = await _quizAttemptService.UpdateAnswerAndScoreAsync(attemptId, request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}