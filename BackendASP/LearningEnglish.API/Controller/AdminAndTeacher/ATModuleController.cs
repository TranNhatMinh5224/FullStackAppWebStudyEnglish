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

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        }

        /// <summary>
        /// Lấy thông tin module theo ID với chi tiết
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
        /// Lấy tất cả module theo lesson ID
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
        /// Tạo module mới
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateModule([FromBody] CreateModuleDto createModuleDto)
        {
            // Kiểm tra validation data đầu vào
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

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetModule), new { moduleId = result.Data!.ModuleId }, result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Cập nhật module (Admin có thể cập nhật bất kỳ, Teacher chỉ module của mình)
        /// </summary>
        [HttpPut("{moduleId}")]
        public async Task<IActionResult> UpdateModule(int moduleId, [FromBody] UpdateModuleDto updateModuleDto)
        {
            // Kiểm tra validation data đầu vào
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

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }

            if (result.StatusCode == 403)
            {
                return StatusCode(403, result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Xóa module (Admin có thể xóa bất kỳ, Teacher chỉ module của mình)
        /// </summary>
        [HttpDelete("{moduleId}")]
        public async Task<IActionResult> DeleteModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _moduleService.DeleteModuleWithAuthorizationAsync(moduleId, userId, userRole);

            if (result.Success)
            {
                return Ok(result);
            }

            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }

            if (result.StatusCode == 403)
            {
                return StatusCode(403, result);
            }

            return BadRequest(result);
        }
    }
}
