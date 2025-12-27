using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LearningEnglish.API.Controller.Admin.AdminQuestion
{
    [Route("api/admin/questions")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminQuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IValidator<QuestionBulkCreateDto> _bulkCreateValidator;
        private readonly ILogger<AdminQuestionController> _logger;

        public AdminQuestionController(
            IQuestionService questionService,
            IValidator<QuestionBulkCreateDto> bulkCreateValidator,
            ILogger<AdminQuestionController> _logger)
        {
            _questionService = questionService;
            _bulkCreateValidator = bulkCreateValidator;
            this._logger = _logger;
        }

        // GET: api/admin/questions/{questionId} - lấy question theo ID
        [HttpGet("{questionId}")]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy Question {QuestionId}", adminId, questionId);

            var result = await _questionService.GetQuestionByIdAsync(questionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/questions/quiz-group/{quizGroupId} - lấy questions by quiz group ID
        [HttpGet("quiz-group/{quizGroupId}")]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> GetQuestionsByQuizGroupId(int quizGroupId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy Questions của QuizGroup {QuizGroupId}", adminId, quizGroupId);

            var result = await _questionService.GetQuestionsByQuizGroupIdAsync(quizGroupId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/questions/quiz-section/{quizSectionId} - lấy questions by quiz section ID
        [HttpGet("quiz-section/{quizSectionId}")]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> GetQuestionsByQuizSectionId(int quizSectionId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy Questions của QuizSection {QuizSectionId}", adminId, quizSectionId);

            var result = await _questionService.GetQuestionsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/questions - tạo mới question
        [HttpPost]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto questionCreateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo Question mới", adminId);

            var result = await _questionService.AddQuestionAsync(questionCreateDto);

            if (!result.Success)
                _logger.LogWarning("Tạo Question thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo Question thành công với ID: {QuestionId}", result.Data?.QuestionId);

            return result.Success
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/questions/bulk - tạo nhiều question
        [HttpPost("bulk")]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> CreateBulkQuestions([FromBody] QuestionBulkCreateDto questionBulkCreateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo bulk Questions ({Count} câu hỏi)", adminId, questionBulkCreateDto.Questions.Count);

            var result = await _questionService.AddBulkQuestionsAsync(questionBulkCreateDto);

            if (!result.Success)
                _logger.LogWarning("Tạo bulk Questions thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo bulk Questions thành công");

            return result.Success
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/questions/{questionId} - cập nhật question
        [HttpPut("{questionId}")]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] QuestionUpdateDto questionUpdateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật Question {QuestionId}", adminId, questionId);

            var result = await _questionService.UpdateQuestionAsync(questionId, questionUpdateDto);

            if (!result.Success)
                _logger.LogWarning("Cập nhật Question thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật Question thành công: {QuestionId}", questionId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/questions/{questionId} - xóa question
        [HttpDelete("{questionId}")]
        [RequirePermission("Admin.Question.Manage")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa Question {QuestionId}", adminId, questionId);

            var result = await _questionService.DeleteQuestionAsync(questionId);

            if (!result.Success)
                _logger.LogWarning("Xóa Question thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa Question thành công: {QuestionId}", questionId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
