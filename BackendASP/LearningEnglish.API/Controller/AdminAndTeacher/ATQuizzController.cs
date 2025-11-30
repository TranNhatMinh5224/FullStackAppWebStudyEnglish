using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/Quiz/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin, Teacher")]
    public class ATQuizzController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public ATQuizzController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        // GET: api/Quiz/ATQuizz/{quizId} - Get quiz by ID (Admin and Teacher)
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {
            var result = await _quizService.GetQuizByIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/Quiz/ATQuizz/create - Create new quiz (Admin and Teacher)
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto quizCreate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _quizService.CreateQuizAsync(quizCreate);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/Quiz/ATQuizz/update/{quizId} - Update quiz (Admin and Teacher)
        [HttpPut("update/{quizId}")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizUpdateDto quizUpdate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _quizService.UpdateQuizAsync(quizId, quizUpdate);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/Quiz/ATQuizz/delete/{quizId} - Delete quiz (Admin and Teacher)
        [HttpDelete("delete/{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {
            var result = await _quizService.DeleteQuizAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/Quiz/ATQuizz/all/{assessmentId} - Get all quizzes in assessment (Admin and Teacher)
        [HttpGet("all/{assessmentId}")]
        public async Task<IActionResult> GetAllQuizzInAssessment(int assessmentId)
        {
            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}