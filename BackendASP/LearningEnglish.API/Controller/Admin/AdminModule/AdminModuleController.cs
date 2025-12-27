using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// Admin quản lý modules

namespace LearningEnglish.API.Controller.Admin
{
    [Route("api/admin/modules")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminModuleController : ControllerBase
    {
        private readonly IAdminModuleService _moduleService;
        private readonly ILogger<AdminModuleController> _logger;

        public AdminModuleController(IAdminModuleService moduleService, ILogger<AdminModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }

        // POST: Admin tạo module mới
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo module cho lesson {LessonId}", adminId, createModuleDto.LessonId);

            var result = await _moduleService.AdminCreateModule(createModuleDto);
            return result.Success
                ? CreatedAtAction(nameof(GetModule), new { moduleId = result.Data!.ModuleId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: Admin xem chi tiết module
        [HttpGet("{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem module {ModuleId}", adminId, moduleId);

            var result = await _moduleService.GetModuleById(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Admin lấy danh sách modules theo lesson
        [HttpGet("lesson/{lessonId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách modules của lesson {LessonId}", adminId, lessonId);

            var result = await _moduleService.GetModulesByLessonId(lessonId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: Admin cập nhật module
        [HttpPut("{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật module {ModuleId}", adminId, moduleId);

            var result = await _moduleService.UpdateModule(moduleId, updateModuleDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Admin xóa module
        [HttpDelete("{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa module {ModuleId}", adminId, moduleId);

            var result = await _moduleService.DeleteModule(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

