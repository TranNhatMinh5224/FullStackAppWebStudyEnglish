using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;


namespace CleanDemo.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/")]
    [Authorize]
    public class ATeacherPackageController : ControllerBase
    {
        private readonly ITeacherPackageService _teacherPackageService;

        public ATeacherPackageController(ITeacherPackageService teacherPackageService)
        {
            _teacherPackageService = teacherPackageService;
        }

        //  CREATE TEACHER PACKAGE
        [HttpPost("admin/Create-Teacher-Package")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTeacherPackage([FromBody] CreateTeacherPackageDto teacherPackageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _teacherPackageService.CreateTeacherPackageAsync(teacherPackageDto);

                if (!result.Success)
                    return BadRequest(new { Message = result.Message });

                return Ok(new { Message = "Teacher package created successfully.", Data = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        //  UPDATE TEACHER PACKAGE
        [HttpPut("admin/Update-Teacher-Package")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacherPackage([FromBody] UpdateTeacherPackageDto teacherPackageDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _teacherPackageService.UpdateTeacherPackageAsync(teacherPackageDto);

                if (!result.Success)
                    return BadRequest(new { Message = result.Message });

                return Ok(new { Message = "Teacher package updated successfully.", Data = result.Data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }

        //  DELETE TEACHER PACKAGE
        [HttpDelete("admin/Delete-Teacher-Package/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTeacherPackage(int id)
        {
            try
            {
                var result = await _teacherPackageService.DeleteTeacherPackageAsync(id);

                if (!result.Success)
                    return BadRequest(new { Message = result.Message });

                return Ok(new { Message = "Teacher package deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Internal server error", Detail = ex.Message });
            }
        }
    }
}
