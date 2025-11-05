using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using System.Security.Claims;
namespace CleanDemo.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/MiniTest/")]
    [Authorize]
    public class ATMiniTestController : ControllerBase
    {
        private readonly IMiniTestService _miniTestService;
        private readonly ILogger<ATMiniTestController> _logger;

        public ATMiniTestController(IMiniTestService miniTestService, ILogger<ATMiniTestController> logger)
        {
            _miniTestService = miniTestService;
            _logger = logger;
        }
        // Controller Admin Thêm MiniTest 
        [HttpPost("admin/create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminAddMiniTest(MiniTestDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);
                var response = await _miniTestService.AdminAddMiniTest(dto);
                if (!response.Success) return BadRequest(response);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating mini test");
                return StatusCode(500, "Internal server error");
            }
        }
        // Controller Teacher Thêm MiniTest
        [HttpPost("teacher/create")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherAddMiniTest(MiniTestDto dto)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return BadRequest("Invalid user ID in token");
                }

                var response = await _miniTestService.TeacherAddMiniTest(dto, teacherId);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding mini test for teacher");
                return StatusCode(500, "Internal server error");
            }
        }
        // Controller Admin Cập Nhật MiniTest
        [HttpPut("admin/update/{miniTestId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminUpdateMiniTest(int miniTestId, [FromBody] UpdateMiniTestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _miniTestService.AdminUpdateMiniTest(miniTestId, dto);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mini test for admin");
                return StatusCode(500, "Internal server error");
            }
        }
        // Controller Teacher Cập Nhật MiniTest
        [HttpPut("teacher/update/{miniTestId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherUpdateMiniTest(int miniTestId, [FromBody] UpdateMiniTestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int teacherId))
                {
                    return BadRequest("Invalid user ID in token");
                }

                var response = await _miniTestService.TeacherUpdateMiniTest(miniTestId, dto, teacherId);

                if (!response.Success)
                    return BadRequest(response);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating mini test for teacher");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

