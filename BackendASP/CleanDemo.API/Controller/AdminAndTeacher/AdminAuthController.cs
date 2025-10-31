using Microsoft.AspNetCore.Mvc;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CleanDemo.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    [Authorize(Roles = "Admin")]

    // ControllerBase là lớp cơ sở cho tất cả các controller trong ASP.NET Core
    public class AdminAuthController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public AdminAuthController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }
        // Controller lấy ra danh sách User trong hệ thống

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userManagementService.GetAllUsersAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
        // Controller lấy ra danh sách giáo viên trong hệ thống
        // Controller block tài khoản người dùng(teacher và student)
        [HttpPut("block-account/{userId}")]
        public async Task<IActionResult> BlockAccount(int userId)
        {
            var result = await _userManagementService.BlockAccountAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(result.Data);
           
        }

    }
}
