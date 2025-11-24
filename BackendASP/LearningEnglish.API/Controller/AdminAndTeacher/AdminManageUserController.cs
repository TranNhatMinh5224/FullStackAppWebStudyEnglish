using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Controller.Admin
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

        // Controller block tài khoản người dùng(teacher và student)
        [HttpPut("block-account/{userId}")]
        public async Task<IActionResult> BlockAccount(int userId)
        {
            var result = await _userManagementService.BlockAccountAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(result.Data);

        }

        // Controller mở khóa tài khoản người dùng
        [HttpPut("unblock-account/{userId}")]
        public async Task<IActionResult> UnblockAccount(int userId)
        {
            var result = await _userManagementService.UnblockAccountAsync(userId);

            if (!result.Success)
                return StatusCode(result.StatusCode, new { message = result.Message });

            return Ok(result.Data);
        }
        // Controller lấy danh sách tài khoản bị khóa
        [HttpGet("list-blocked-accounts")]
        public async Task<IActionResult> GetListBlockedAccounts()
        {
            var result = await _userManagementService.GetListBlockedAccountsAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

        // Controller lấy danh sách giáo viên trong hệ thống
        [HttpGet("teachers")]

        public async Task<IActionResult> GetListTeachers()
        {
            var result = await _userManagementService.GetListTeachersAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
        // Controller lấy danh sách học sinh theo all course
        [HttpGet("getall-students-by-all-courses")]
        public async Task<IActionResult> GetStudentsByAllCourses()
        {
            var result = await _userManagementService.GetStudentsByAllCoursesAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }
    }
}
