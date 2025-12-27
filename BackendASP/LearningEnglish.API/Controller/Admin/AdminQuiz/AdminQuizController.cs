using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin.AdminQuiz
{
    [Route("api/admin/quizzes")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminQuizController : ControllerBase
    {
        private readonly IAdminQuizService _quizService;
        private readonly ILogger<AdminQuizController> _logger;

        public AdminQuizController(IAdminQuizService quizService, ILogger<AdminQuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        // POST: api/admin/quizzes - tạo mới quiz
        [HttpPost]
        [RequirePermission("Admin.Quiz.Manage")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto quizCreate)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang tạo Quiz mới", userId);

            var result = await _quizService.CreateQuizAsync(quizCreate);

            if (!result.Success)
                _logger.LogWarning("Tạo Quiz thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo Quiz thành công với ID: {QuizId}", result.Data?.QuizId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quizzes/{quizId} - lấy quiz theo ID
        [HttpGet("{quizId}")]
        [RequirePermission("Admin.Quiz.Manage")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang lấy Quiz {QuizId}", userId, quizId);

            var result = await _quizService.GetQuizByIdAsync(quizId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quizzes/assessment/{assessmentId} - lấy tất cả quiz theo assessment ID
        [HttpGet("assessment/{assessmentId}")]
        [RequirePermission("Admin.Quiz.Manage")]
        public async Task<IActionResult> GetAllQuizzInAssessment(int assessmentId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang lấy danh sách Quiz cho Assessment {AssessmentId}", userId, assessmentId);

            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/quizzes/{quizId} - cập nhật quiz
        [HttpPut("{quizId}")]
        [RequirePermission("Admin.Quiz.Manage")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizUpdateDto quizUpdate)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang cập nhật Quiz {QuizId}", userId, quizId);

            var result = await _quizService.UpdateQuizAsync(quizId, quizUpdate);


            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/quizzes/{quizId} - xóa quiz
        [HttpDelete("{quizId}")]
        [RequirePermission("Admin.Quiz.Manage")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang xóa Quiz {QuizId}", userId, quizId);

            var result = await _quizService.DeleteQuizAsync(quizId);


            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
