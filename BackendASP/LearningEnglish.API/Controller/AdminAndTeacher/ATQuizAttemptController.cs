using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;


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

        // Lấy danh sách tất cả attempts của một quiz
        [HttpGet("quiz/{quizId}")]
        public async Task<IActionResult> GetQuizAttempts(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptsAsync(quizId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Lấy chi tiết một attempt
        [HttpGet("{attemptId}")]
        public async Task<IActionResult> GetAttemptDetails(int attemptId)
        {
            var result = await _quizAttemptAdminService.GetAttemptDetailsAsync(attemptId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        // Force submit một attempt (cho admin)
        [Authorize(Roles = "Admin")]
        [HttpPost("force-submit/{attemptId}")]
        public async Task<IActionResult> ForceSubmitAttempt(int attemptId)
        {
            var result = await _quizAttemptAdminService.ForceSubmitAttemptAsync(attemptId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Lấy thống kê attempts của một quiz
        [HttpGet("stats/{quizId}")]
        public async Task<IActionResult> GetQuizAttemptStats(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizAttemptStatsAsync(quizId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Lấy danh sách điểm của các user đã làm quiz
        [HttpGet("scores/{quizId}")]
        public async Task<IActionResult> GetQuizScores(int quizId)
        {
            var result = await _quizAttemptAdminService.GetQuizScoresAsync(quizId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        // Lấy lịch sử attempts của user cho một quiz
        [HttpGet("user/{userId}/quiz/{quizId}")]
        public async Task<IActionResult> GetUserQuizAttempts(int userId, int quizId)
        {
            var result = await _quizAttemptAdminService.GetUserQuizAttemptsAsync(userId, quizId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}
