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
    
        [HttpPost("start/{quizId}")]
        public async Task<IActionResult> StartQuizAttempt(int quizId)
        {
            var userId = User.GetUserId();

            var result = await _quizAttemptService.StartQuizAttemptAsync(quizId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/User/QuizAttempt/submit/{attemptId} - nop bai quiz
        [HttpPost("submit/{attemptId}")]
        public async Task<IActionResult> SubmitQuizAttempt(int attemptId)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.SubmitQuizAttemptAsync(attemptId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/QuizAttempt/resume/{attemptId} - tiep tuc lam bai quiz khi chua nop
        [HttpGet("resume/{attemptId}")]
        public async Task<IActionResult> ResumeQuizAttempt(int attemptId)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.ResumeQuizAttemptAsync(attemptId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpPost("update-answer/{attemptId}")]
        public async Task<IActionResult> UpdateAnswerAndScore(int attemptId, [FromBody] UpdateAnswerRequestDto request)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.UpdateAnswerAndScoreAsync(attemptId, request, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/quiz-attempts/check-active/{quizId} - kiem tra co bai lam dang thuc hien khong
        [HttpGet("check-active/{quizId}")]
        public async Task<IActionResult> CheckActiveAttempt(int quizId)
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.CheckActiveAttemptAsync(quizId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/quiz-attempts/current-active - kiem tra xem co dang lam bat ky bai quiz nao khong
        [HttpGet("current-active")]
        public async Task<IActionResult> GetAnyActiveAttempt()
        {
            var userId = User.GetUserId();
            var result = await _quizAttemptService.GetAnyActiveAttemptAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}