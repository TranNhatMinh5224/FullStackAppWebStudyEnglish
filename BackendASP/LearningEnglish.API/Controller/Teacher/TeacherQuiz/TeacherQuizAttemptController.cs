using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.Teacher
{
    [ApiController]
    [Route("api/teacher/quiz-attempts")]
    [Authorize(Roles = "Teacher")]
    public class TeacherQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptTeacherService _quizAttemptTeacherService;

        public TeacherQuizAttemptController(IQuizAttemptTeacherService quizAttemptTeacherService)
        {
            _quizAttemptTeacherService = quizAttemptTeacherService;
        }

        // GET: api/teacher/quiz-attempts/quiz/{quizId}/paged - Lấy danh sách attempts của một quiz (CÓ PHÂN TRANG)
        [HttpGet("quiz/{quizId}/paged")]
        public async Task<IActionResult> GetQuizAttemptsPaged(int quizId, [FromQuery] PageRequest request)
        {
            var teacherId = User.GetUserId();
            var pagedResult = await _quizAttemptTeacherService.GetQuizAttemptsPagedAsync(quizId, teacherId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/teacher/quiz-attempts/quiz/{quizId} - Lấy danh sách attempts của một quiz (KHÔNG PHÂN TRANG)
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var teacherId = User.GetUserId();
            var result = await _quizAttemptTeacherService.GetQuizAttemptsAsync(quizId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-attempts/stats/{quizId} - Lấy thống kê làm bài cho một quiz
        [HttpGet("stats/{quizId}")]
        public async Task<IActionResult> GetQuizAttemptStats(int quizId)
        {
            var teacherId = User.GetUserId();
            var result = await _quizAttemptTeacherService.GetQuizAttemptStatsAsync(quizId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-attempts/scores/{quizId}/paged - Lấy điểm của các user đã hoàn thành quiz (CÓ PHÂN TRANG)
        [HttpGet("scores/{quizId}/paged")]
        public async Task<IActionResult> GetQuizScoresPaged(int quizId, [FromQuery] PageRequest request)
        {
            var teacherId = User.GetUserId();
            var pagedResult = await _quizAttemptTeacherService.GetQuizScoresPagedAsync(quizId, teacherId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // GET: api/teacher/quiz-attempts/scores/{quizId} - Lấy điểm của các user đã hoàn thành quiz (KHÔNG PHÂN TRANG)
        [HttpGet("scores/{quizId}")]
        public async Task<IActionResult> GetQuizScores(int quizId)
        {
            var teacherId = User.GetUserId();
            var result = await _quizAttemptTeacherService.GetQuizScoresAsync(quizId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-attempts/user/{userId}/quiz/{quizId} - Lấy tất cả các attempts của một user cho một quiz
        [HttpGet("user/{userId}/quiz/{quizId}")]
        public async Task<IActionResult> GetUserQuizAttempts(int userId, int quizId)
        {
            var teacherId = User.GetUserId();
            var result = await _quizAttemptTeacherService.GetUserQuizAttemptsAsync(userId, quizId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-attempts/{attemptId}/review - Lấy chi tiết bài làm với đáp án để review
        [HttpGet("{attemptId}/review")]
        public async Task<IActionResult> GetAttemptDetailForReview(int attemptId)
        {
            var teacherId = User.GetUserId();
            var result = await _quizAttemptTeacherService.GetAttemptDetailForReviewAsync(attemptId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/quiz-attempts/force-submit/{attemptId} - Bắt buộc nộp bài (khi học sinh vi phạm, quên nộp)
        [HttpPost("force-submit/{attemptId}")]
        public async Task<IActionResult> ForceSubmitAttempt(int attemptId)
        {
            var teacherId = User.GetUserId();
            var result = await _quizAttemptTeacherService.ForceSubmitAttemptAsync(attemptId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
