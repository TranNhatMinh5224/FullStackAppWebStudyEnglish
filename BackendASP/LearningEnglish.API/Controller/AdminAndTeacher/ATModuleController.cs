using LearningEnglish.Application.DTOS;
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

        /// <summary>
        /// Get module by ID with details
        /// </summary>
        [HttpGet("{moduleId}")]
        public async Task<IActionResult> GetModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModuleByIdAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get all modules by lesson ID
        /// </summary>
        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetModulesByLesson(int lessonId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetModulesByLessonIdAsync(lessonId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Create new module
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _moduleService.CreateModuleAsync(createModuleDto, userId);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetModule), new { moduleId = result.Data!.ModuleId }, result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Update existing module
        /// </summary>
        [HttpPut("{moduleId}")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _moduleService.UpdateModuleAsync(moduleId, updateModuleDto, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Delete module
        /// </summary>
        [HttpDelete("{moduleId}")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.DeleteModuleAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Reorder modules in a lesson
        /// </summary>
        [HttpPut("lesson/{lessonId}/reorder")]
        public async Task<IActionResult> ReorderModules(int lessonId, [FromBody] List<ReorderModuleDto> reorderItems)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _moduleService.ReorderModulesAsync(lessonId, reorderItems, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get next module
        /// </summary>
        [HttpGet("{moduleId}/next")]
        public async Task<IActionResult> GetNextModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetNextModuleAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Get previous module
        /// </summary>
        [HttpGet("{moduleId}/previous")]
        public async Task<IActionResult> GetPreviousModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.GetPreviousModuleAsync(moduleId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Bulk delete modules
        /// </summary>
        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDeleteModules([FromBody] List<int> moduleIds)
        {
            if (moduleIds == null || !moduleIds.Any())
            {
                return BadRequest("Module IDs are required");
            }

            var userId = GetCurrentUserId();
            var result = await _moduleService.BulkDeleteModulesAsync(moduleIds, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Duplicate modules to another lesson
        /// </summary>
        [HttpPost("duplicate")]
        public async Task<IActionResult> DuplicateModules([FromBody] BulkModuleOperationDto bulkOperation)
        {
            if (!ModelState.IsValid || !bulkOperation.TargetLessonId.HasValue)
            {
                return BadRequest("Target lesson ID is required for duplication");
            }

            var userId = GetCurrentUserId();
            var result = await _moduleService.DuplicateModulesToLessonAsync(
                bulkOperation.ModuleIds, 
                bulkOperation.TargetLessonId.Value, 
                userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Check if user can access module
        /// </summary>
        [HttpGet("{moduleId}/access")]
        public async Task<IActionResult> CheckModuleAccess(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.CanUserAccessModuleAsync(moduleId, userId);

            return Ok(result);
        }

        /// <summary>
        /// Check if user can manage module
        /// </summary>
        [HttpGet("{moduleId}/manage")]
        public async Task<IActionResult> CheckModuleManagement(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _moduleService.CanUserManageModuleAsync(moduleId, userId);

            return Ok(result);
        }
    }
}
