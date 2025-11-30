using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/AdminTeacher/QuizAttempt")]
    [Authorize(Roles = "Admin,Teacher")]
    public class ATQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptAdminService _quizAttemptAdminService;

        public ATQuizAttemptController(IQuizAttemptAdminService quizAttemptAdminService)
        {
            _quizAttemptAdminService = quizAttemptAdminService;
        }

        // GET: api/AdminTeacher/QuizAttempt/quiz/{quizId} - Lấy tất cả các lần làm bài của một quiz
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/AdminTeacher/QuizAttempt/{attemptId} - Lấy chi tiết một lần làm bài
        [HttpGet("{attemptId}")]
        public async Task<IActionResult> GetAttemptDetails(int attemptId)
        {
            var result = await _quizAttemptAdminService.GetAttemptDetailsAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/AdminTeacher/QuizAttempt/force-submit/{attemptId} - Bắt buộc nộp bài (chỉ Admin)
        [Authorize(Roles = "Admin")]
        [HttpPost("force-submit/{attemptId}")]
        public async Task<IActionResult> ForceSubmitAttempt(int attemptId)
        {
            var result = await _quizAttemptAdminService.ForceSubmitAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/AdminTeacher/QuizAttempt/stats/{quizId} - Lấy thống kê các lần làm bài của quiz
        [HttpGet("stats/{quizId}")]
        public async Task<IActionResult> GetQuizAttemptStats(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptStatsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/AdminTeacher/QuizAttempt/scores/{quizId} - Lấy điểm của các user đã hoàn thành quiz
        [HttpGet("scores/{quizId}")]
        public async Task<IActionResult> GetQuizScores(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizScoresAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/AdminTeacher/QuizAttempt/user/{userId}/quiz/{quizId} - Lấy lịch sử làm bài của user cho một quiz
        [HttpGet("user/{userId}/quiz/{quizId}")]
        public async Task<IActionResult> GetUserQuizAttempts(int userId, int quizId)
        {
            var result = await _quizAttemptAdminService.GetUserQuizAttemptsAsync(userId, quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
