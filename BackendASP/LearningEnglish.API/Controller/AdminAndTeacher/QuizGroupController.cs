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

        // POST: api/QuizGroup - Create quiz group
        [HttpPost]
        public async Task<IActionResult> CreateQuizGroup([FromBody] CreateQuizGroupDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _quizGroupService.CreateQuizGroupAsync(createDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/QuizGroup/{id} - Get quiz group by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizGroup(int id)
        {
            var result = await _quizGroupService.GetQuizGroupByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/QuizGroup/by-quiz-section/{quizSectionId} - Get quiz groups by quiz section ID
        [HttpGet("by-quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuizGroupsByQuizSectionId(int quizSectionId)
        {
            var result = await _quizGroupService.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/QuizGroup/{id} - Update quiz group
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizGroup(int id, [FromBody] UpdateQuizGroupDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _quizGroupService.UpdateQuizGroupAsync(id, updateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/QuizGroup/{id} - Delete quiz group
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizGroup(int id)
        {
            var result = await _quizGroupService.DeleteQuizGroupAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
