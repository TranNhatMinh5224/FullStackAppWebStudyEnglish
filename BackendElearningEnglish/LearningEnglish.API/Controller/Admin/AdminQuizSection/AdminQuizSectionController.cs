using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin.AdminQuizSection
{
    [ApiController]
    [Route("api/admin/quiz-sections")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminQuizSectionController : ControllerBase
    {
        private readonly IQuizSectionService _quizSectionService;
        private readonly IQuestionService _questionService;
        private readonly ILogger<AdminQuizSectionController> _logger;

        public AdminQuizSectionController(
            IQuizSectionService quizSectionService,
            IQuestionService questionService,
            ILogger<AdminQuizSectionController> logger)
        {
            _quizSectionService = quizSectionService;
            _questionService = questionService;
            _logger = logger;
        }

        // POST: api/admin/quiz-sections - tạo mới quiz section
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateQuizSection([FromBody] CreateQuizSectionDto createDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo QuizSection mới", adminId);

            var result = await _quizSectionService.CreateQuizSectionAsync(createDto);

            if (!result.Success)
                _logger.LogWarning("Tạo QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo QuizSection thành công với ID: {QuizSectionId}", result.Data?.QuizSectionId);

            return result.Success
                ? CreatedAtAction(nameof(GetQuizSectionById), new { id = result.Data?.QuizSectionId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-sections/{id} - lấy quiz section theo ID
        [HttpGet("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizSectionById(int id)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy QuizSection {QuizSectionId}", adminId, id);

            var result = await _quizSectionService.GetQuizSectionByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-sections/by-quiz/{quizId} - lấy danh sách quiz sections theo quiz ID
        [HttpGet("by-quiz/{quizId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizSectionsByQuizId(int quizId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy QuizSections của Quiz {QuizId}", adminId, quizId);

            var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/quiz-sections/{id} - cập nhật quiz section
        [HttpPut("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateQuizSection(int id, [FromBody] UpdateQuizSectionDto updateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật QuizSection {QuizSectionId}", adminId, id);

            var result = await _quizSectionService.UpdateQuizSectionAsync(id, updateDto);

            if (!result.Success)
                _logger.LogWarning("Cập nhật QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật QuizSection thành công: {QuizSectionId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/quiz-sections/{id} - xóa quiz section
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteQuizSection(int id)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa QuizSection {QuizSectionId}", adminId, id);

            var result = await _quizSectionService.DeleteQuizSectionAsync(id);

            if (!result.Success)
                _logger.LogWarning("Xóa QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa QuizSection thành công: {QuizSectionId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/quiz-sections/bulk - Bulk tạo section với groups và questions
        [HttpPost("bulk")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateQuizSectionBulk([FromBody] QuizSectionBulkCreateDto bulkCreateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang bulk tạo QuizSection với {GroupCount} groups", 
                adminId, bulkCreateDto.QuizGroups?.Count ?? 0);

            var result = await _questionService.CreateQuizSectionBulkAsync(bulkCreateDto);

            if (!result.Success)
                _logger.LogWarning("Bulk tạo QuizSection thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Bulk tạo QuizSection thành công với ID: {QuizSectionId}", result.Data?.QuizSectionId);

            return result.Success
                ? StatusCode(201, result)
                : StatusCode(result.StatusCode, result);
        }
    }
}
