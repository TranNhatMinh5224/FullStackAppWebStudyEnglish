using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.Admin.AdminQuizGroup
{
    [ApiController]
    [Route("api/admin/quiz-groups")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminQuizGroupController : ControllerBase
    {
        private readonly IQuizGroupService _quizGroupService;
        private readonly ILogger<AdminQuizGroupController> _logger;

        public AdminQuizGroupController(IQuizGroupService quizGroupService, ILogger<AdminQuizGroupController> logger)
        {
            _quizGroupService = quizGroupService;
            _logger = logger;
        }

        // POST: api/admin/quiz-groups - tạo mới quiz group
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateQuizGroup([FromBody] CreateQuizGroupDto createDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo QuizGroup mới", adminId);

            var result = await _quizGroupService.CreateQuizGroupAsync(createDto);

            if (!result.Success)
                _logger.LogWarning("Tạo QuizGroup thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo QuizGroup thành công với ID: {QuizGroupId}", result.Data?.QuizGroupId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-groups/{id} - lấy quiz group theo ID
        [HttpGet("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizGroup(int id)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy QuizGroup {QuizGroupId}", adminId, id);

            var result = await _quizGroupService.GetQuizGroupByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/quiz-groups/by-quiz-section/{quizSectionId} - lấy danh sách quiz groups theo quiz section ID
        [HttpGet("by-quiz-section/{quizSectionId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetQuizGroupsByQuizSectionId(int quizSectionId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang lấy QuizGroups của QuizSection {QuizSectionId}", adminId, quizSectionId);

            var result = await _quizGroupService.GetQuizGroupsByQuizSectionIdAsync(quizSectionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/quiz-groups/{id} - cập nhật quiz group
        [HttpPut("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateQuizGroup(int id, [FromBody] UpdateQuizGroupDto updateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật QuizGroup {QuizGroupId}", adminId, id);

            var result = await _quizGroupService.UpdateQuizGroupAsync(id, updateDto);

            if (!result.Success)
                _logger.LogWarning("Cập nhật QuizGroup thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Cập nhật QuizGroup thành công: {QuizGroupId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/quiz-groups/{id} - xóa quiz group
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteQuizGroup(int id)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa QuizGroup {QuizGroupId}", adminId, id);

            var result = await _quizGroupService.DeleteQuizGroupAsync(id);

            if (!result.Success)
                _logger.LogWarning("Xóa QuizGroup thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Xóa QuizGroup thành công: {QuizGroupId}", id);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
