using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Teacher")]
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

        // GET: api/Question/{questionId} - Get question by ID
        [HttpGet("{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var result = await _questionService.GetQuestionByIdAsync(questionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/Question/quiz-group/{quizGroupId} - Get questions by quiz group ID
        [HttpGet("quiz-group/{quizGroupId}")]
        public async Task<IActionResult> GetQuestionsByQuizGroupId(int quizGroupId)
        {
            var result = await _questionService.GetQuestionsByQuizGroupIdAsync(quizGroupId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/Question/quiz-section/{quizSectionId} - Get questions by quiz section ID
        [HttpGet("quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuestionsByQuizSectionId(int quizSectionId)
        {
            var result = await _questionService.GetQuestionsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/Question/create - Create new question with answers
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto questionCreateDto)
        {
            var result = await _questionService.AddQuestionAsync(questionCreateDto);
            return result.Success 
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/Question/bulk-create - Bulk create questions with answers
        [HttpPost("bulk-create")]
        public async Task<IActionResult> CreateBulkQuestions([FromBody] QuestionBulkCreateDto questionBulkCreateDto)
        {
            var result = await _questionService.AddBulkQuestionsAsync(questionBulkCreateDto);
            return result.Success 
                ? StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/Question/update/{questionId} - Update question
        [HttpPut("update/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] QuestionUpdateDto questionUpdateDto)
        {
            var result = await _questionService.UpdateQuestionAsync(questionId, questionUpdateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/Question/delete/{questionId} - Delete question
        [HttpDelete("delete/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var result = await _questionService.DeleteQuestionAsync(questionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
