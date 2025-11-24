using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/admin/quiz-sections")]
    [Authorize(Roles = "Admin,Teacher")]
    public class QuizSectionController : ControllerBase
    {
        private readonly IQuizSectionService _quizSectionService;

        public QuizSectionController(IQuizSectionService quizSectionService)
        {
            _quizSectionService = quizSectionService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateQuizSection([FromBody] CreateQuizSectionDto createDto)
        {
            try
            {
                // FluentValidation tự động validate với ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _quizSectionService.CreateQuizSectionAsync(createDto);

                if (result.Success)
                    return CreatedAtAction(nameof(GetQuizSectionById), new { id = result.Data?.QuizSectionId }, result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizSectionById(int id)
        {
            try
            {
                var result = await _quizSectionService.GetQuizSectionByIdAsync(id);

                if (result.Success)
                    return Ok(result);

                return NotFound(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Lấy danh sách phần quiz theo Quiz ID
        [HttpGet("by-quiz/{quizId}")]
        public async Task<IActionResult> GetQuizSectionsByQuizId(int quizId)
        {
            try
            {
                var result = await _quizSectionService.GetQuizSectionsByQuizIdAsync(quizId);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizSection(int id, [FromBody] UpdateQuizSectionDto updateDto)
        {
            try
            {
                // FluentValidation tự động validate với ModelState
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _quizSectionService.UpdateQuizSectionAsync(id, updateDto);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Xóa phần quiz
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizSection(int id)
        {
            try
            {
                var result = await _quizSectionService.DeleteQuizSectionAsync(id);

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
