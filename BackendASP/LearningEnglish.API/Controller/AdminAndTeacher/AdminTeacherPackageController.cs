using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/teacher-packages")]
    [Authorize(Roles = "Admin")]
    public class AdminTeacherPackageController : ControllerBase
    {
        private readonly ITeacherPackageService _teacherPackageService;

        public AdminTeacherPackageController(ITeacherPackageService teacherPackageService)
        {
            _teacherPackageService = teacherPackageService;
        }

        // GET: api/admin/teacher-packages - lấy tat cả teacher package
        [HttpGet]
        public async Task<IActionResult> GetAllTeacherPackages()
        {
            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // GET: api/admin/teacher-packages/{id} - lấy teacher package theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid ID provided." });

            var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // POST: api/admin/teacher-packages - tạo mới teacher package
        [HttpPost]
        public async Task<IActionResult> CreateTeacherPackage([FromBody] CreateTeacherPackageDto createDto)
        {
            var result = await _teacherPackageService.CreateTeacherPackageAsync(createDto);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // PUT: api/admin/teacher-packages/{id} - sửa gói giao viên
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeacherPackage(int id, [FromBody] UpdateTeacherPackageDto teacherPackageDto)
        {

            var result = await _teacherPackageService.UpdateTeacherPackageAsync(id, teacherPackageDto);
            return result.Success
                ? Ok(new { Message = "Teacher package updated successfully.", Data = result.Data })
                : StatusCode(result.StatusCode, new { Message = result.Message });
        }

        // DELETE: api/admin/teacher-packages/{id} - xoá gói giáo viên
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacherPackage(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid ID provided." });

            var result = await _teacherPackageService.DeleteTeacherPackageAsync(id);
            return result.Success
                ? Ok(new { message = result.Message, data = result.Data })
                : StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
