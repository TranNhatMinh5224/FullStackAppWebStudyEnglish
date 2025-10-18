using Microsoft.AspNetCore.Mvc;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CleanDemo.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/teacher-packages")]
    public class AdminTeacherPackageController : ControllerBase
    {
        private readonly ITeacherPackageService _teacherPackageService;

        public AdminTeacherPackageController(ITeacherPackageService teacherPackageService)
        {
            _teacherPackageService = teacherPackageService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTeacherPackages()
        {
            var result = await _teacherPackageService.GetAllTeacherPackagesAsync();
            if (!result.Success) 
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

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

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacherPackage(int id, [FromBody] UpdateTeacherPackageDto updateDto)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            if (id <= 0)
                return BadRequest(new { message = "Invalid ID provided." });

            // Đảm bảo ID trong DTO khớp với ID trong URL
            updateDto.TeacherPackageId = id;

            var result = await _teacherPackageService.UpdateTeacherPackageAsync(updateDto);
            if (!result.Success) 
                return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

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