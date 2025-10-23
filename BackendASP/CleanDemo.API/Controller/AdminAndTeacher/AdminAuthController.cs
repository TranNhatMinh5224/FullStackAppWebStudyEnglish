using Microsoft.AspNetCore.Mvc;
using CleanDemo.Application.Interface;
using CleanDemo.Application.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace CleanDemo.API.Controllers.Admin
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

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userManagementService.GetAllUsersAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(result.Data);
        }

    


    }
}
