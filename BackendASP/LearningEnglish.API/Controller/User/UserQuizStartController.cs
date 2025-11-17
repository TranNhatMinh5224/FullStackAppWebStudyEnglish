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

        /// <summary>
        /// Update câu trả lời và tính điểm ngay lập tức (real-time scoring)
        /// </summary>
        /// <param name="attemptId">ID của quiz attempt</param>
        /// <param name="request">
        /// Format của UserAnswer theo từng loại câu hỏi:
        /// - MultipleChoice/TrueFalse: int (optionId) hoặc string "1" → {"questionId": 1, "userAnswer": 1}
        /// - MultipleAnswers: List&lt;int&gt; hoặc array → {"questionId": 1, "userAnswer": [1, 2, 3]}
        /// - FillBlank: string → {"questionId": 1, "userAnswer": "answer text"}
        /// - Matching: Dictionary&lt;int, int&gt; → {"questionId": 1, "userAnswer": {"1": 2, "3": 4}}
        /// - Ordering: List&lt;int&gt; → {"questionId": 1, "userAnswer": [3, 1, 2, 4]}
        /// </param>
        /// <returns>Điểm của câu hỏi này (0 nếu sai, Points nếu đúng)</returns>
        /// <remarks>
        /// Khi user làm câu nào, sẽ update answer và chấm điểm luôn.
        /// Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm.
        /// </remarks>
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