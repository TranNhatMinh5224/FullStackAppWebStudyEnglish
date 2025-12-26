using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/quiz-groups")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class QuizGroupController : ControllerBase
    {
        private readonly IQuizGroupService _quizGroupService;

        public QuizGroupController(IQuizGroupService quizGroupService)
        {
            _quizGroupService = quizGroupService;
        }

        // POST: api/quiz-groups - tạo mới quiz group
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo quiz group cho quiz sections của own courses
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateQuizGroup([FromBody] CreateQuizGroupDto createDto)
        {
            var result = await _quizGroupService.CreateQuizGroupAsync(createDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/quiz-groups/{id} - lấy quiz group theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizGroup(int id)
        {
            var result = await _quizGroupService.GetQuizGroupByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/QuizGroup/by-quiz-section/{quizSectionId} - lấy danh sách quiz groups theo quiz section ID
        [HttpGet("by-quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuizGroupsByQuizSectionId(int quizSectionId)
        {
            var result = await _quizGroupService.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/quiz-groups/{id} - sửa quiz group
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ sửa quiz group của own courses (RLS check)
        [HttpPut("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateQuizGroup(int id, [FromBody] UpdateQuizGroupDto updateDto)
        {
            var result = await _quizGroupService.UpdateQuizGroupAsync(id, updateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/quiz-groups/{id} - xoá quiz group
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ xóa quiz group của own courses (RLS check)
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteQuizGroup(int id)
        {
            var result = await _quizGroupService.DeleteQuizGroupAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
