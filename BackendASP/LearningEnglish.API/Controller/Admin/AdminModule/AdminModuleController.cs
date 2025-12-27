using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin
{
    [Route("api/admin/modules")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ILogger<AdminModuleController> _logger;

        public AdminModuleController(IModuleService moduleService, ILogger<AdminModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }

        // POST: api/admin/modules - Admin tạo module
        // RLS: modules_policy_admin_all (Admin có quyền tạo module trong bất kỳ lesson nào)
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

        // GET: api/admin/modules/{moduleId} - Admin xem chi tiết module
        // RLS: modules_policy_admin_all (Admin có quyền xem tất cả modules)
        [HttpGet("{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem module {ModuleId}", adminId, moduleId);

            // RLS đã filter: Admin có quyền xem tất cả modules
            var result = await _moduleService.GetModuleByIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/modules/lesson/{lessonId} - Admin xem danh sách modules theo lesson
        // RLS: modules_policy_admin_all (Admin có quyền xem tất cả modules)
        [HttpGet("lesson/{lessonId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách modules của lesson {LessonId}", adminId, lessonId);

            // RLS đã filter: Admin có quyền xem tất cả modules
            // userId = null vì Admin không cần progress info
            var result = await _moduleService.GetModulesByLessonIdAsync(lessonId, null);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/modules/{moduleId} - Admin cập nhật module
        // RLS: modules_policy_admin_all (Admin có quyền cập nhật tất cả modules)
        [HttpPut("{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật module {ModuleId}", adminId, moduleId);

            // RLS đã filter: Admin có quyền cập nhật tất cả modules
            var result = await _moduleService.UpdateModule(moduleId, updateModuleDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/modules/{moduleId} - Admin xóa module
        // RLS: modules_policy_admin_all (Admin có quyền xóa tất cả modules)
        [HttpDelete("{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa module {ModuleId}", adminId, moduleId);

            // RLS đã filter: Admin có quyền xóa tất cả modules
            var result = await _moduleService.DeleteModule(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

