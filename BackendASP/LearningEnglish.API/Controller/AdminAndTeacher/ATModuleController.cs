using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ATModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;
        private readonly ILogger<ATModuleController> _logger;

        public ATModuleController(IModuleService moduleService, ILogger<ATModuleController> logger)
        {
            _moduleService = moduleService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        // GET: api/ATModule/{moduleId} - Get module by ID with details
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModuleByIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/ATModule/lesson/{lessonId} - Get all modules by lesson ID
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModulesByLessonIdAsync(lessonId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/ATModule - Create new module
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    success = false,
                    statusCode = 400,
                    message = "Dữ liệu đầu vào không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var userId = GetCurrentUserId();
            var result = await _moduleService.CreateModuleAsync(createModuleDto, userId);
            return result.Success 
                ? CreatedAtAction(nameof(GetModule), new { moduleId = result.Data!.ModuleId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/ATModule/{moduleId} - Update module (Admin: any module, Teacher: own modules only)
        [HttpPut("{moduleId}")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                { 
                    success = false,
                    statusCode = 400,
                    message = "Dữ liệu đầu vào không hợp lệ",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _moduleService.UpdateModuleWithAuthorizationAsync(moduleId, updateModuleDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/ATModule/{moduleId} - Delete module (Admin: any module, Teacher: own modules only)
        [HttpDelete("{moduleId}")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _moduleService.DeleteModuleWithAuthorizationAsync(moduleId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
