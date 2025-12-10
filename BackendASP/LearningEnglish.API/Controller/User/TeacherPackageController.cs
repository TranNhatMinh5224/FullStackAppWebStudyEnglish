using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/teacher-packages")]
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

        // GET: api/user/teacher-packages - Retrieve all available teacher packages for purchase
        [HttpGet("teacher-packages")]
        public async Task<IActionResult> GetTeacherPackages()
        {
            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/teacher-packages/{id} - Get detailed information about a specific teacher package
        [HttpGet("teacher-packages/{id}")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "Invalid package ID" });
            }

            var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
