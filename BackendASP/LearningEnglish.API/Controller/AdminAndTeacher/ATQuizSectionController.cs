using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/quiz-sections")]
    [Authorize(Roles = "Admin,Teacher")]
    public class QuizSectionController : ControllerBase
    {
        private readonly IQuizSectionService _quizSectionService;

        public QuizSectionController(IQuizSectionService quizSectionService)
        {
            _quizSectionService = quizSectionService;
        }

        // POST: api/admin/quiz-sections - Create a new quiz section
        [HttpPost]
        public async Task<IActionResult> CreateQuizSection([FromBody] CreateQuizSectionDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _quizSectionService.CreateQuizSectionAsync(createDto);
            return result.Success
                ? CreatedAtAction(nameof(GetQuizSectionById), new { id = result.Data?.QuizSectionId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-sections/{id} - Get quiz section by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizSectionById(int id)
        {
            var result = await _quizSectionService.GetQuizSectionByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-sections/by-quiz/{quizId} - Get all quiz sections by quiz ID
        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetQuizSectionsByQuizId(int quizId)
        {
            var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/quiz-sections/{id} - Update quiz section
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizSection(int id, [FromBody] UpdateQuizSectionDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _quizSectionService.UpdateQuizSectionAsync(id, updateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/quiz-sections/{id} - Delete quiz section
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizSection(int id)
        {
            var result = await _quizSectionService.DeleteQuizSectionAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
