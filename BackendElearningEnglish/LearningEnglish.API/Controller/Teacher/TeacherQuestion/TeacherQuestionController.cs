using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LearningEnglish.API.Controller.Teacher.TeacherQuestion
{
    [Route("api/teacher/questions")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherQuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IValidator<QuestionBulkCreateDto> _bulkCreateValidator;
        private readonly ILogger<TeacherQuestionController> _logger;

        public TeacherQuestionController(
            IQuestionService questionService,
            IValidator<QuestionBulkCreateDto> bulkCreateValidator,
            ILogger<TeacherQuestionController> logger)
        {
            _questionService = questionService;
            _bulkCreateValidator = bulkCreateValidator;
            _logger = logger;
        }

        // GET: api/teacher/questions/{questionId} - lấy question theo ID (own courses only)
        [HttpGet("{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy Question {QuestionId}", teacherId, questionId);

            var result = await _questionService.GetQuestionByIdAsync(questionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/questions/quiz-group/{quizGroupId} - lấy questions by quiz group ID (own courses only)
        [HttpGet("quiz-group/{quizGroupId}")]
        public async Task<IActionResult> GetQuestionsByQuizGroupId(int quizGroupId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy Questions của QuizGroup {QuizGroupId}", teacherId, quizGroupId);

            var result = await _questionService.GetQuestionsByQuizGroupIdAsync(quizGroupId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/questions/quiz-section/{quizSectionId} - lấy questions by quiz section ID (own courses only)
        [HttpGet("quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuestionsByQuizSectionId(int quizSectionId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy Questions của QuizSection {QuizSectionId}", teacherId, quizSectionId);

            var result = await _questionService.GetQuestionsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/questions - tạo mới question (own courses only)
        [HttpPost]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto questionCreateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo Question mới", teacherId);

            var result = await _questionService.AddQuestionAsync(questionCreateDto);

            if (!result.Success)
                _logger.LogWarning("Tạo Question thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo Question thành công với ID: {QuestionId}", result.Data?.QuestionId);

            return result.Success
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/questions/bulk - tạo nhiều question (own courses only)
        [HttpPost("bulk")]
        public async Task<IActionResult> CreateBulkQuestions([FromBody] QuestionBulkCreateDto questionBulkCreateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo bulk Questions ({Count} câu hỏi)", teacherId, questionBulkCreateDto.Questions.Count);

            var result = await _questionService.AddBulkQuestionsAsync(questionBulkCreateDto);

            if (!result.Success)
                _logger.LogWarning("Tạo bulk Questions thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo bulk Questions thành công");

            return result.Success
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/questions/{questionId} - cập nhật question (own courses only)
        [HttpPut("{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] QuestionUpdateDto questionUpdateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật Question {QuestionId}", teacherId, questionId);

            var result = await _questionService.UpdateQuestionAsync(questionId, questionUpdateDto);

            if (!result.Success)
                _logger.LogWarning("Cập nhật Question thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật Question thành công: {QuestionId}", questionId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/questions/{questionId} - xóa question (own courses only)
        [HttpDelete("{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa Question {QuestionId}", teacherId, questionId);

            var result = await _questionService.DeleteQuestionAsync(questionId);

            if (!result.Success)
                _logger.LogWarning("Xóa Question thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa Question thành công: {QuestionId}", questionId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
