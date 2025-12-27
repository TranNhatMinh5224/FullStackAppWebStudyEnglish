using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.TeacherPackage;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/teacher-packages")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminTeacherPackageController : ControllerBase
    {
        private readonly ITeacherPackageService _teacherPackageService;
        private readonly ILogger<AdminTeacherPackageController> _logger;

        public AdminTeacherPackageController(
            ITeacherPackageService teacherPackageService,
            ILogger<AdminTeacherPackageController> logger)
        {
            _teacherPackageService = teacherPackageService;
            _logger = logger;
        }

        // GET: Admin lấy danh sách tất cả gói giáo viên
        [HttpGet]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> GetAllTeacherPackages()
        {
            _logger.LogInformation("Admin lấy danh sách teacher packages");

            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Admin xem chi tiết gói giáo viên theo ID
        [HttpGet("{id}")]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            _logger.LogInformation("Admin xem chi tiết teacher package {PackageId}", id);

            var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: Admin tạo mới gói giáo viên
        [HttpPost]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> CreateTeacherPackage([FromBody] CreateTeacherPackageDto createDto)
        {
            _logger.LogInformation("Admin tạo teacher package: {PackageName}", createDto.PackageName);

            var result = await _teacherPackageService.CreateTeacherPackageAsync(createDto);
            
          
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: Admin cập nhật gói giáo viên
        [HttpPut("{id}")]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> UpdateTeacherPackage(int id, [FromBody] UpdateTeacherPackageDto updateDto)
        {
            _logger.LogInformation("Admin cập nhật teacher package {PackageId}", id);

            var result = await _teacherPackageService.UpdateTeacherPackageAsync(id, updateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Admin xóa gói giáo viên
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> DeleteTeacherPackage(int id)
        {
            _logger.LogInformation("Admin xóa teacher package {PackageId}", id);

            var result = await _teacherPackageService.DeleteTeacherPackageAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
