using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    [Authorize(Roles = "Admin")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public AdminAuthController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        // GET: api/admin/auth/users - Get all users in the system
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userManagementService.GetAllUsersAsync();
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // PUT: api/admin/auth/block-account/{userId} - Block user account (Teacher or Student)
        [HttpPut("block-account/{userId}")]
        public async Task<IActionResult> BlockAccount(int userId)
        {
            var result = await _userManagementService.BlockAccountAsync(userId);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // PUT: api/admin/auth/unblock-account/{userId} - Unblock user account
        [HttpPut("unblock-account/{userId}")]
        public async Task<IActionResult> UnblockAccount(int userId)
        {
            var result = await _userManagementService.UnblockAccountAsync(userId);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // GET: api/admin/auth/list-blocked-accounts - Get all blocked accounts
        [HttpGet("list-blocked-accounts")]
        public async Task<IActionResult> GetListBlockedAccounts()
        {
            var result = await _userManagementService.GetListBlockedAccountsAsync();
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // GET: api/admin/auth/teachers - Get all teachers in the system
        [HttpGet("teachers")]
        public async Task<IActionResult> GetListTeachers()
        {
            var result = await _userManagementService.GetListTeachersAsync();
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // GET: api/admin/auth/getall-students-by-all-courses - Get students across all courses
        [HttpGet("getall-students-by-all-courses")]
        public async Task<IActionResult> GetStudentsByAllCourses()
        {
            var result = await _userManagementService.GetStudentsByAllCoursesAsync();
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
