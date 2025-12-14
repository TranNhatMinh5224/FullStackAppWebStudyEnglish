using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Common.Pagination;
using Microsoft.AspNetCore.Authorization;

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "Admin")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;

        public AdminAuthController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        // GET: api/admin/users/users - Get all users with pagination
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PageRequest request)
        {
            var pagedResult = await _userManagementService.GetAllUsersPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
        }

        // PUT: api/admin/users/block-account/{userId} - Block user account (Teacher or Student)
        [HttpPut("block-account/{userId}")]
        public async Task<IActionResult> BlockAccount(int userId)
        {
            var result = await _userManagementService.BlockAccountAsync(userId);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // PUT: api/admin/users/unblock-account/{userId} - Unblock user account
        [HttpPut("unblock-account/{userId}")]
        public async Task<IActionResult> UnblockAccount(int userId)
        {
            var result = await _userManagementService.UnblockAccountAsync(userId);
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }

        // GET: api/admin/users/list-blocked-accounts - Get all blocked accounts with pagination
        [HttpGet("list-blocked-accounts")]
        public async Task<IActionResult> GetListBlockedAccounts([FromQuery] PageRequest request)
        {
            var pagedResult = await _userManagementService.GetListBlockedAccountsPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
        }
    
        // GET: api/admin/users/teachers - Get all teachers in the system with pagination
        [HttpGet("teachers")]
        public async Task<IActionResult> GetListTeachers([FromQuery] PageRequest request)
        { 
            var pagedResult = await _userManagementService.GetListTeachersPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
        }
    }
}
