using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/quiz-attempts")]
    [Authorize(Roles = "Student")]
    public class UserQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _quizAttemptService;

        public UserQuizAttemptController(IQuizAttemptService quizAttemptService)
        {
            _quizAttemptService = quizAttemptService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // POST: api/User/QuizAttempt/start/{quizId} - Start quiz attempt
        [HttpPost("start/{quizId}")]
        public async Task<IActionResult> StartQuizAttempt(int quizId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user ID" });

            var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/User/QuizAttempt/submit/{attemptId} - Submit quiz attempt
        [HttpPost("submit/{attemptId}")]
        public async Task<IActionResult> SubmitQuizAttempt(int attemptId)
        {
            var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/QuizAttempt/resume/{attemptId} - Resume quiz attempt after disconnect
        [HttpGet("resume/{attemptId}")]
        public async Task<IActionResult> ResumeQuizAttempt(int attemptId)
        {
            var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/User/QuizAttempt/update-answer/{attemptId} - Update answer and calculate score in real-time
        // UserAnswer format by question type:
        // - MultipleChoice/TrueFalse: int (optionId) → {"questionId": 1, "userAnswer": 1}
        // - MultipleAnswers: List<int> → {"questionId": 1, "userAnswer": [1, 2, 3]}
        // - FillBlank: string → {"questionId": 1, "userAnswer": "answer text"}
        // - Matching: Dictionary<int, int> → {"questionId": 1, "userAnswer": {"1": 2, "3": 4}}
        // - Ordering: List<int> → {"questionId": 1, "userAnswer": [3, 1, 2, 4]}
        [HttpPost("update-answer/{attemptId}")]
        public async Task<IActionResult> UpdateAnswerAndScore(int attemptId, [FromBody] UpdateAnswerRequestDto request)
        {
            var result = await _quizAttemptService.UpdateAnswerAndScoreAsync(attemptId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}