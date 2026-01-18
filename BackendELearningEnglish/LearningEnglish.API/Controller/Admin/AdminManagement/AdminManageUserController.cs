using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;
using Microsoft.Extensions.Logging;


// Admin quản lý người dùng

namespace LearningEnglish.API.Controller.Admin
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminManageUserController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly IAdminManagementService _adminManagementService;
        private readonly ILogger<AdminManageUserController> _logger;

        public AdminManageUserController(
            IUserManagementService userManagementService,
            IAdminManagementService adminManagementService,
            ILogger<AdminManageUserController> logger)
        {
            _userManagementService = userManagementService;
            _adminManagementService = adminManagementService;
            _logger = logger;
        }

        // endpoint Admin lấy tất cả người dùng với phân trang
        [HttpGet]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters request)
        {
            var pagedResult = await _userManagementService.GetAllUsersPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // endpoint Admin khóa tài khoản người dùng
        [HttpPut("block/{userId}")]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> BlockAccount(int userId)
        {
            var result = await _userManagementService.BlockAccountAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin mở khóa tài khoản người dùng
        [HttpPut("unblock/{userId}")]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> UnblockAccount(int userId)
        {
            var result = await _userManagementService.UnblockAccountAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy danh sách tài khoản bị khóa với phân trang
        [HttpGet("blocked")]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> GetListBlockedAccounts([FromQuery] UserQueryParameters request)
        {
            var pagedResult = await _userManagementService.GetListBlockedAccountsPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // endpoint Admin lấy danh sách giáo viên với phân trang
        [HttpGet("teachers")]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> GetListTeachers([FromQuery] UserQueryParameters request)
        {
            var pagedResult = await _userManagementService.GetListTeachersPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // endpoint Admin nâng cấp user thành Teacher 
        
        [HttpPost("upgrade-to-teacher")]
        [RequirePermission("Admin.User.Manage", "Admin.Package.Manage")]
        public async Task<IActionResult> UpgradeUserToTeacher([FromBody] UpgradeUserToTeacherDto dto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang nâng cấp user {Email} thành Teacher với package {PackageId}", 
                adminId, dto.Email, dto.TeacherPackageId);

            var result = await _adminManagementService.UpgradeUserToTeacherAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy chi tiết user theo ID
        [HttpGet("{userId:int}")]
        [RequirePermission("Admin.User.Manage")]
        public async Task<IActionResult> GetUserById(int userId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem chi tiết user {UserId}", adminId, userId);

            var result = await _userManagementService.GetUserByIdAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

