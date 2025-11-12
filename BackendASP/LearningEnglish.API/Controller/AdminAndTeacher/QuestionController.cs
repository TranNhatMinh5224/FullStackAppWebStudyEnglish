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


        // Lấy câu hỏi theo ID

        [HttpGet("{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var result = await _questionService.GetQuestionByIdAsync(questionId);

            if (result.Success)
                return Ok(result);

            return NotFound(result);
        }


        //Lấy danh sách câu hỏi theo QuizGroupId

        [HttpGet("quiz-group/{quizGroupId}")]
        public async Task<IActionResult> GetQuestionsByQuizGroupId(int quizGroupId)
        {
            var result = await _questionService.GetQuestionsByQuizGroupIdAsync(quizGroupId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        // Lấy danh sách câu hỏi theo QuizSectionId

        [HttpGet("quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuestionsByQuizSectionId(int quizSectionId)
        {
            var result = await _questionService.GetQuestionsByQuizSectionIdAsync(quizSectionId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }


        /// Tạo câu hỏi mới (kèm đáp án)

        [HttpPost("create")]
        public async Task<IActionResult> CreateQuestion([FromBody] QuestionCreateDto questionCreateDto)
        {

            var result = await _questionService.AddQuestionAsync(questionCreateDto);

            if (result.Success)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result);

            return BadRequest(result);
        }


        // Tạo hàng loạt câu hỏi (Bulk Create) - Gộp cả Question + Answer Options

        [HttpPost("bulk-create")]
        public async Task<IActionResult> CreateBulkQuestions([FromBody] QuestionBulkCreateDto questionBulkCreateDto)
        {

            var result = await _questionService.AddBulkQuestionsAsync(questionBulkCreateDto);

            if (result.Success)
                return StatusCode(result.StatusCode > 0 ? result.StatusCode : 201, result);

            return BadRequest(result);
        }

        /// <summary>
        /// Cập nhật câu hỏi
        /// </summary>
        [HttpPut("update/{questionId}")]
        public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] QuestionUpdateDto questionUpdateDto)
        {


            var result = await _questionService.UpdateQuestionAsync(questionId, questionUpdateDto);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// Xóa câu hỏi
        /// </summary>
        [HttpDelete("delete/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            var result = await _questionService.DeleteQuestionAsync(questionId);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
