using LearningEnglish.Application.DTOs;

using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        [HttpGet("{quizId}")]
        public async Task<IActionResult> GetQuiz(int quizId)
        {

            var result = await _quizService.GetQuizByIdAsync(quizId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        // Tạo quiz 
        [HttpPost("create")]
        public async Task<IActionResult> CreateQuiz([FromBody] QuizCreateDto quizCreate)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _quizService.CreateQuizAsync(quizCreate);
            if (result.Success)
            {
                return Ok(result);
            }



            return BadRequest(result);
        }
        [HttpPut("update/{quizId}")]
        public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] QuizUpdateDto quizUpdate)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _quizService.UpdateQuizAsync(quizId, quizUpdate);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpDelete("delete/{quizId}")]
        public async Task<IActionResult> DeleteQuiz(int quizId)
        {

            var result = await _quizService.DeleteQuizAsync(quizId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpGet("all/{assessmentId}")]
        public async Task<IActionResult> GetAllQuizzInAssessment(int assessmentId)
        {
            var result = await _quizService.GetQuizzesByAssessmentIdAsync(assessmentId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }

}