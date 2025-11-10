using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/")]
    [Authorize]
    public class TeacherPackageController : ControllerBase
    {
        private readonly ITeacherPackageService _teacherPackageService;
        private readonly ILogger<TeacherPackageController> _logger;

        public TeacherPackageController(ITeacherPackageService teacherPackageService, ILogger<TeacherPackageController> logger)
        {
            _teacherPackageService = teacherPackageService;
            _logger = logger;
        }


        // Lấy danh sách tất cả gói teacher (cho user xem để mua)

        [HttpGet("teacher-packages")]
        public async Task<IActionResult> GetTeacherPackages()
        {
            try
            {
                var result = await _teacherPackageService.GetAllTeacherPackagesAsync();

                if (!result.Success)
                {
                    _logger.LogWarning("Failed to get teacher packages: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("User retrieved {Count} teacher packages", result.Data?.Count ?? 0);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTeacherPackages");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        
        // Lấy chi tiết gói teacher theo ID
        
        [HttpGet("teacher-packages/{id}")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new { message = "Invalid package ID" });
                }

                var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);

                if (!result.Success)
                {
                    _logger.LogWarning("Teacher package {Id} not found: {Message}", id, result.Message);
                    return NotFound(new { message = result.Message });
                }

                _logger.LogInformation("User retrieved teacher package {Id}", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTeacherPackageById for Id: {Id}", id);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
