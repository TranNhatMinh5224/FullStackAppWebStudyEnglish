using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/modules")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class ATModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ILogger<ATModuleController> _logger;

        public ATModuleController(IModuleService moduleService, ILogger<ATModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }


        // GET: api/modules/{moduleId} - lấy module theo ID
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _moduleService.GetModuleByIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/modules/lesson/{lessonId} - lấy tất cả module theo lesson ID
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var userId = User.GetUserId();
            var result = await _moduleService.GetModulesByLessonIdAsync(lessonId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/ATModule - tạo mới module
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ tạo module cho lessons của own courses
        [HttpPost]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _moduleService.CreateModuleAsync(createModuleDto, userId, userRole);
            return result.Success
                ? CreatedAtAction(nameof(GetModule), new { moduleId = result.Data!.ModuleId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/ATModule/{moduleId} - sửa module
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ sửa module của own courses (RLS check)
        [HttpPut("{moduleId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _moduleService.UpdateModuleWithAuthorizationAsync(moduleId, updateModuleDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/ATModule/{moduleId} - xoá module
        // Admin: Cần permission Admin.Lesson.Manage
        // Teacher: Chỉ xóa module của own courses (RLS check)
        [HttpDelete("{moduleId}")]
        [RequirePermission("Admin.Lesson.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _moduleService.DeleteModuleWithAuthorizationAsync(moduleId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
