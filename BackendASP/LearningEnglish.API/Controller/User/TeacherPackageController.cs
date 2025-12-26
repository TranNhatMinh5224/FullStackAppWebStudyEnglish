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
    [AllowAnonymous] 
    public class TeacherPackageController : ControllerBase
    {
        private readonly ITeacherPackageService _teacherPackageService;
        private readonly ILogger<TeacherPackageController> _logger;

        public TeacherPackageController(ITeacherPackageService teacherPackageService, ILogger<TeacherPackageController> logger)
        {
            _teacherPackageService = teacherPackageService;
            _logger = logger;
        }

        // endpoint Guest/User lấy danh sách tất cả gói giáo viên
        // TeacherPackages là public catalog - không cần RLS, mọi người đều có thể xem
        [HttpGet]
        public async Task<IActionResult> GetTeacherPackages()
        {
            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Guest/User lấy chi tiết gói giáo viên theo ID
        // TeacherPackages là public catalog - không cần RLS, mọi người đều có thể xem
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
