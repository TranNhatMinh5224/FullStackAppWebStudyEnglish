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

        // Update câu trả lời và tính điểm ngay lập tức (real-time scoring)
        // Format của UserAnswer theo từng loại câu hỏi:
        // - MultipleChoice/TrueFalse: int (optionId) → {"questionId": 1, "userAnswer": 1}
        // - MultipleAnswers: List<int> → {"questionId": 1, "userAnswer": [1, 2, 3]}
        // - FillBlank: string → {"questionId": 1, "userAnswer": "answer text"}
        // - Matching: Dictionary<int, int> → {"questionId": 1, "userAnswer": {"1": 2, "3": 4}}
        // - Ordering: List<int> → {"questionId": 1, "userAnswer": [3, 1, 2, 4]}
        // Khi user làm câu nào, sẽ update answer và chấm điểm luôn.
        // Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm.
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