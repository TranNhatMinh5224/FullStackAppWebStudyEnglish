using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/quiz-attempts")]
    [Authorize(Roles = "Admin,Teacher")]
    public class ATQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptAdminService _quizAttemptAdminService;

        public ATQuizAttemptController(IQuizAttemptAdminService quizAttemptAdminService)
        {
            _quizAttemptAdminService = quizAttemptAdminService;
        }

        /// <summary>
        /// Lấy danh sách attempts của một quiz (CÓ PHÂN TRANG)
        /// PageRequest có giá trị mặc định, luôn dùng phân trang
        /// </summary>
        [HttpGet("quiz/{quizId}/paged")]
        public async Task<IActionResult> GetQuizAttemptsPaged(int quizId, [FromQuery] PageRequest request)
        {
            var pagedResult = await _quizAttemptAdminService.GetQuizAttemptsPagedAsync(quizId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        /// <summary>
        /// Lấy danh sách attempts của một quiz (KHÔNG PHÂN TRANG)
        /// Trả về tất cả attempts
        /// </summary>
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy chi tiết một lần làm bài
        /// </summary>
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

        /// <summary>
        /// Lấy thống kê các lần làm bài của quiz
        /// </summary>
        [HttpGet("stats/{quizId}")]
        public async Task<IActionResult> GetQuizAttemptStats(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptStatsAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy điểm của các user đã hoàn thành quiz (CÓ PHÂN TRANG)
        /// PageRequest có giá trị mặc định, luôn dùng phân trang
        /// </summary>
        [HttpGet("scores/{quizId}/paged")]
        public async Task<IActionResult> GetQuizScoresPaged(int quizId, [FromQuery] PageRequest request)
        {
            var pagedResult = await _quizAttemptAdminService.GetQuizScoresPagedAsync(quizId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        /// <summary>
        /// Lấy điểm của các user đã hoàn thành quiz (KHÔNG PHÂN TRANG)
        /// Trả về tất cả điểm
        /// </summary>
        [HttpGet("scores/{quizId}")]
        public async Task<IActionResult> GetQuizScores(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizScoresAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy lịch sử làm bài của user cho một quiz
        /// </summary>
        [HttpGet("user/{userId}/quiz/{quizId}")]
        public async Task<IActionResult> GetUserQuizAttempts(int userId, int quizId)
        {
            var result = await _quizAttemptAdminService.GetUserQuizAttemptsAsync(userId, quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
