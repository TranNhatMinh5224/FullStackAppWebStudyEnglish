using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/quiz-attempts")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class ATQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptAdminService _quizAttemptAdminService;

        public ATQuizAttemptController(IQuizAttemptAdminService quizAttemptAdminService)
        {
            _quizAttemptAdminService = quizAttemptAdminService;
        }

        // GET: api/quiz-attempts/quiz/{quizId}/paged - Lấy danh sách attempts của một quiz (CÓ PHÂN TRANG)
        [HttpGet("quiz/{quizId}/paged")]
        public async Task<IActionResult> GetQuizAttemptsPaged(int quizId, [FromQuery] PageRequest request)
        {
            var pagedResult = await _quizAttemptAdminService.GetQuizAttemptsPagedAsync(quizId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/quiz-attempts/quiz/{quizId} - Lấy danh sách attempts của một quiz (KHÔNG PHÂN TRANG)
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/quiz-attempts/{attemptId} - Lấy chi tiết một attempt theo ID
        [HttpGet("{attemptId}")]
        public async Task<IActionResult> GetAttemptDetails(int attemptId)
        {
            var result = await _quizAttemptAdminService.GetAttemptDetailsAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/AdminTeacher/QuizAttempt/force-submit/{attemptId} - Bắt buộc nộp bài
        // Admin: Cần permission Admin.Content.Manage
        [HttpPost("force-submit/{attemptId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> ForceSubmitAttempt(int attemptId)
        {
            var result = await _quizAttemptAdminService.ForceSubmitAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/quiz-attempts/stats/{quizId} - Lấy thống kê làm bài cho một quiz
        [HttpGet("stats/{quizId}")]
        public async Task<IActionResult> GetQuizAttemptStats(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptStatsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/quiz-attempts/scores/{quizId}/paged - Lấy điểm của các user đã hoàn thành quiz (CÓ PHÂN TRANG)
        [HttpGet("scores/{quizId}/paged")]
        public async Task<IActionResult> GetQuizScoresPaged(int quizId, [FromQuery] PageRequest request)
        {
            var pagedResult = await _quizAttemptAdminService.GetQuizScoresPagedAsync(quizId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/quiz-attempts/scores/{quizId} - Lấy điểm của các user đã hoàn thành quiz (KHÔNG PHÂN TRANG)
        [HttpGet("scores/{quizId}")]
        public async Task<IActionResult> GetQuizScores(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizScoresAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/quiz-attempts/user/{userId}/quiz/{quizId} - Lấy tất cả các attempts của một user cho một quiz
        [HttpGet("user/{userId}/quiz/{quizId}")]
        public async Task<IActionResult> GetUserQuizAttempts(int userId, int quizId)
        {
            var result = await _quizAttemptAdminService.GetUserQuizAttemptsAsync(userId, quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
