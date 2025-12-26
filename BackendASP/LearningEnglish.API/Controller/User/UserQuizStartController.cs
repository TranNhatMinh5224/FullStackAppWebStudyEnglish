using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;
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

        // POST: api/User/QuizAttempt/start/{quizId} - bắt đầu làm bài quiz
        // RLS: quizattempts_policy_student_all_own
        [HttpPost("start/{quizId}")]
        public async Task<IActionResult> StartQuizAttempt(int quizId)
        {
            var userId = User.GetUserId();

            var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/User/QuizAttempt/submit/{attemptId} - nop bai quiz
        // RLS: quizattempts_policy_student_all_own (chỉ update attempts của chính mình)
        [HttpPost("submit/{attemptId}")]
        public async Task<IActionResult> SubmitQuizAttempt(int attemptId)
        {
            // RLS sẽ filter quiz attempts theo userId
            var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/QuizAttempt/resume/{attemptId} - tiep tuc lam bai quiz khi chua nop
        // RLS: quizattempts_policy_student_all_own (chỉ xem attempts của chính mình)
        [HttpGet("resume/{attemptId}")]
        public async Task<IActionResult> ResumeQuizAttempt(int attemptId)
        {
            // RLS sẽ filter quiz attempts theo userId
            var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/User/QuizAttempt/update-answer/{attemptId} - cập nhật câu trả lời và điểm số cho một câu hỏi trong bài quiz
        // RLS: quizattempts_policy_student_all_own (chỉ update attempts của chính mình)
        // FluentValidation: UpdateAnswerRequestDto validator sẽ tự động validate
        [HttpPost("update-answer/{attemptId}")]
        public async Task<IActionResult> UpdateAnswerAndScore(int attemptId, [FromBody] UpdateAnswerRequestDto request)
        {
            // RLS sẽ filter quiz attempts theo userId
            var result = await _quizAttemptService.UpdateAnswerAndScoreAsync(attemptId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}