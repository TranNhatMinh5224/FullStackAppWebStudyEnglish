using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;

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

        // Lấy tất cả các lần làm bài của một quiz
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizAttempts(int quizId, [FromQuery] PageRequest? request)
        {
            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _quizAttemptAdminService.GetQuizAttemptsPagedAsync(quizId, request);
                return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
            }

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

        // Lấy điểm của các user đã hoàn thành quiz
        [HttpGet("scores/{quizId}")]
        public async Task<IActionResult> GetQuizScores(int quizId, [FromQuery] PageRequest? request)
        {
            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _quizAttemptAdminService.GetQuizScoresPagedAsync(quizId, request);
                return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
            }

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
