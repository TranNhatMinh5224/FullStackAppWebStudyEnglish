using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Controllers.Admin
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

        // Controller lấy ra danh sách gói giáo viên trong hệ thống

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTeacherPackages()
        {
            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
        // Controller lấy ra gói giáo viên theo ID

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTeacherPackageById(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid ID provided." });

            var result = await _teacherPackageService.GetTeacherPackageByIdAsync(id);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
        // Controller tạo mới gói giáo viên

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacherPackage([FromBody] CreateTeacherPackageDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _teacherPackageService.CreateTeacherPackageAsync(createDto);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
        // Controller cập nhật gói giáo viên

        [HttpPut("Update-Teacher-Package{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacherPackage(int id, [FromBody] UpdateTeacherPackageDto teacherPackageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _teacherPackageService.UpdateTeacherPackageAsync(id, teacherPackageDto);

                if (!result.Success)
                    return BadRequest(new { Message = result.Message });

                return Ok(new { Message = "Teacher package updated successfully.", Data = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }
        // Controller xóa gói giáo viên
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacherPackage(int id)
        {
            if (id <= 0)
                return BadRequest(new { message = "Invalid ID provided." });

            var result = await _teacherPackageService.DeleteTeacherPackageAsync(id);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return NoContent();
        }
    }
}
