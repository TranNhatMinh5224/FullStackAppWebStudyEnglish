using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/quiz-attempts")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptAdminService _quizAttemptAdminService;

        public AdminQuizAttemptController(IQuizAttemptAdminService quizAttemptAdminService)
        {
            _quizAttemptAdminService = quizAttemptAdminService;
        }

        // GET: api/admin/quiz-attempts/quiz/{quizId}/paged - Lấy danh sách attempts của một quiz (CÓ PHÂN TRANG)
        [HttpGet("quiz/{quizId}/paged")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizAttemptsPaged(int quizId, [FromQuery] PageRequest request)
        {
            var pagedResult = await _quizAttemptAdminService.GetQuizAttemptsPagedAsync(quizId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/admin/quiz-attempts/quiz/{quizId} - Lấy danh sách attempts của một quiz (KHÔNG PHÂN TRANG)
        [HttpGet("quiz/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-attempts/{attemptId}/review - Lấy chi tiết bài làm với đáp án để review
        [HttpGet("{attemptId}/review")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetAttemptDetailForReview(int attemptId)
        {
            var result = await _quizAttemptAdminService.GetAttemptDetailForReviewAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/quiz-attempts/force-submit/{attemptId} - Bắt buộc nộp bài
        [HttpPost("force-submit/{attemptId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> ForceSubmitAttempt(int attemptId)
        {
            var result = await _quizAttemptAdminService.ForceSubmitAttemptAsync(attemptId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-attempts/stats/{quizId} - Lấy thống kê làm bài cho một quiz
        [HttpGet("stats/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizAttemptStats(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptStatsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-attempts/scores/{quizId}/paged - Lấy điểm của các user đã hoàn thành quiz (CÓ PHÂN TRANG)
        [HttpGet("scores/{quizId}/paged")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizScoresPaged(int quizId, [FromQuery] PageRequest request)
        {
            var pagedResult = await _quizAttemptAdminService.GetQuizScoresPagedAsync(quizId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/admin/quiz-attempts/scores/{quizId} - Lấy điểm của các user đã hoàn thành quiz (KHÔNG PHÂN TRANG)
        [HttpGet("scores/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizScores(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizScoresAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-attempts/user/{userId}/quiz/{quizId} - Lấy tất cả các attempts của một user cho một quiz
        [HttpGet("user/{userId}/quiz/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetUserQuizAttempts(int userId, int quizId)
        {
            var result = await _quizAttemptAdminService.GetUserQuizAttemptsAsync(userId, quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

