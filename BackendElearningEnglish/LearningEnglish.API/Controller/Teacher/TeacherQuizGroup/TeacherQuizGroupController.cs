using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.Teacher.TeacherQuizGroup
{
    [ApiController]
    [Route("api/teacher/quiz-groups")]
    [RequireTeacherRole]
    public class TeacherQuizGroupController : ControllerBase
    {
        private readonly IQuizGroupService _quizGroupService;
        private readonly ILogger<TeacherQuizGroupController> _logger;

        public TeacherQuizGroupController(IQuizGroupService quizGroupService, ILogger<TeacherQuizGroupController> logger)
        {
            _quizGroupService = quizGroupService;
            _logger = logger;
        }

        // POST: api/teacher/quiz-groups - tạo mới quiz group (own courses only)
        [HttpPost]
        public async Task<IActionResult> CreateQuizGroup([FromBody] CreateQuizGroupDto createDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo QuizGroup mới", teacherId);

            var result = await _quizGroupService.CreateQuizGroupAsync(createDto);

            if (!result.Success)
                _logger.LogWarning("Tạo QuizGroup thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo QuizGroup thành công với ID: {QuizGroupId}", result.Data?.QuizGroupId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-groups/{id} - lấy quiz group theo ID (own courses only)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizGroup(int id)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy QuizGroup {QuizGroupId}", teacherId, id);

            var result = await _quizGroupService.GetQuizGroupByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/quiz-groups/by-quiz-section/{quizSectionId} - lấy danh sách quiz groups theo quiz section ID (own courses only)
        [HttpGet("by-quiz-section/{quizSectionId}")]
        public async Task<IActionResult> GetQuizGroupsByQuizSectionId(int quizSectionId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy QuizGroups của QuizSection {QuizSectionId}", teacherId, quizSectionId);

            var result = await _quizGroupService.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/quiz-groups/{id} - cập nhật quiz group (own courses only)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuizGroup(int id, [FromBody] UpdateQuizGroupDto updateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật QuizGroup {QuizGroupId}", teacherId, id);

            var result = await _quizGroupService.UpdateQuizGroupAsync(id, updateDto);

            if (!result.Success)
                _logger.LogWarning("Cập nhật QuizGroup thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật QuizGroup thành công: {QuizGroupId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/quiz-groups/{id} - xóa quiz group (own courses only)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuizGroup(int id)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa QuizGroup {QuizGroupId}", teacherId, id);

            var result = await _quizGroupService.DeleteQuizGroupAsync(id);

            if (!result.Success)
                _logger.LogWarning("Xóa QuizGroup thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa QuizGroup thành công: {QuizGroupId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
