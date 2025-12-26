using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
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

        public AdminTeacherPackageController(ITeacherPackageService teacherPackageService)
        {
            _teacherPackageService = teacherPackageService;
        }

        // endpoint Admin lấy danh sách tất cả gói giáo viên
        // TeacherPackages là public catalog - không cần RLS, chỉ cần Permission check
        [HttpGet]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> GetAllTeacherPackages()
        {
            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy chi tiết gói giáo viên theo ID
        // TeacherPackages là public catalog - không cần RLS, chỉ cần Permission check
        [HttpGet("{id}")]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            // Validation: Service layer sẽ xử lý invalid id (trả về 404)
            var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin tạo mới gói giáo viên
        // TeacherPackages là public catalog - không cần RLS, chỉ cần Permission check
        [HttpPost]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> CreateTeacherPackage([FromBody] CreateTeacherPackageDto createDto)
        {
            // FluentValidation: CreateTeacherPackageDtoValidator sẽ tự động validate
            var result = await _teacherPackageService.CreateTeacherPackageAsync(createDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin cập nhật gói giáo viên
        // TeacherPackages là public catalog - không cần RLS, chỉ cần Permission check
        [HttpPut("{id}")]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> UpdateTeacherPackage(int id, [FromBody] UpdateTeacherPackageDto teacherPackageDto)
        {
            // FluentValidation: UpdateTeacherPackageDtoValidator sẽ tự động validate
            // Validation: Service layer sẽ xử lý invalid id (trả về 404)
            var result = await _teacherPackageService.UpdateTeacherPackageAsync(id, teacherPackageDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin xóa gói giáo viên
        // TeacherPackages là public catalog - không cần RLS, chỉ cần Permission check
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Package.Manage")]
        public async Task<IActionResult> DeleteTeacherPackage(int id)
        {
            // Validation: Service layer sẽ xử lý invalid id (trả về 404)
            var result = await _teacherPackageService.DeleteTeacherPackageAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
