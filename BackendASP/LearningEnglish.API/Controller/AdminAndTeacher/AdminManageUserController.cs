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

        // GET: api/admin/auth/users - Get all users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] PageRequest? request)
        {
            // Nếu có parameters phân trang, gọi service phân trang
            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _userManagementService.GetAllUsersPagedAsync(request);
                return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
            }

            // Không có parameters, trả về tất cả
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

        // Lấy danh sách học sinh theo tất cả khóa học
        [HttpGet("getall-students-by-all-courses")]
        public async Task<IActionResult> GetStudentsByAllCourses([FromQuery] PageRequest? request)
        {
            if (request != null && (request.PageNumber > 1 || request.PageSize != 20 || !string.IsNullOrEmpty(request.SearchTerm)))
            {
                var pagedResult = await _userManagementService.GetStudentsByAllCoursesPagedAsync(request);
                return pagedResult.Success ? Ok(pagedResult.Data) : StatusCode(pagedResult.StatusCode, new { message = pagedResult.Message });
            }

            var result = await _userManagementService.GetStudentsByAllCoursesAsync();
            return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
        }
    }
}
