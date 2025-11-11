using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class QuizGroupController : ControllerBase
    {
        private readonly IQuizGroupService _quizGroupService;

        public QuizGroupController(IQuizGroupService quizGroupService)
        {
            _quizGroupService = quizGroupService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuizGroup([FromBody] CreateQuizGroupDto createDto)
        {
            // FluentValidation tự động validate với ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _quizGroupService.CreateQuizGroupAsync(createDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizGroup(int id)
        {
            var result = await _quizGroupService.GetQuizGroupByIdAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return NotFound(result);
        }

        [HttpGet("by-quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuizGroupsByQuizSectionId(int quizSectionId)
        {
            var result = await _quizGroupService.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizGroup(int id, [FromBody] UpdateQuizGroupDto updateDto)
        {
            // FluentValidation tự động validate với ModelState
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _quizGroupService.UpdateQuizGroupAsync(id, updateDto);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizGroup(int id)
        {
            var result = await _quizGroupService.DeleteQuizGroupAsync(id);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
    }
}
