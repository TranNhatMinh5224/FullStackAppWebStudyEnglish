using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/questions")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class QuestionController : ControllerBase
    {
        private readonly IQuestionService _questionService;
        private readonly IValidator<QuestionBulkCreateDto> _bulkCreateValidator;

        public QuestionController(
            IQuestionService questionService,
            IValidator<QuestionBulkCreateDto> bulkCreateValidator)
        {
            _questionService = questionService;
            _bulkCreateValidator = bulkCreateValidator;
        }

        // GET: api/questions/{questionId} - lấy question theo ID
        [HttpGet("{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var result = await _questionService.GetQuestionByIdAsync(questionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/questions/quiz-group/{quizGroupId} - lấy questions by quiz group ID
        [HttpGet("quiz-group/{quizGroupId}")]
        public async Task<IActionResult> GetQuestionsByQuizGroupId(int quizGroupId)
        {
            var result = await _questionService.GetQuestionsByQuizGroupIdAsync(quizGroupId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/questions/quiz-section/{quizSectionId} - lấy questions by quiz section ID
        [HttpGet("quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuestionsByQuizSectionId(int quizSectionId)
        {
            var result = await _questionService.GetQuestionsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/questions - tạo mới question
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo question cho quiz groups của own courses
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto questionCreateDto)
        {
            var result = await _questionService.AddQuestionAsync(questionCreateDto);
            return result.Success
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/questions/bulk - tạo nhiều question
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo question cho quiz groups của own courses
        [HttpPost("bulk")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateBulkQuestions([FromBody] QuestionBulkCreateDto questionBulkCreateDto)
        {
            var result = await _questionService.AddBulkQuestionsAsync(questionBulkCreateDto);
            return result.Success
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/questions/{questionId} - sửa question
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ sửa question của own courses (RLS check)
        [HttpPut("{questionId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] QuestionUpdateDto questionUpdateDto)
        {
            var result = await _questionService.UpdateQuestionAsync(questionId, questionUpdateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/questions/{questionId} - xoá question
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ xóa question của own courses (RLS check)
        [HttpDelete("{questionId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var result = await _questionService.DeleteQuestionAsync(questionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
