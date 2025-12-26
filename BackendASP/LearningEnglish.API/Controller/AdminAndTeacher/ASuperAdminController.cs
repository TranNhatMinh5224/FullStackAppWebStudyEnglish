using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs.Admin;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/superadmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly IAdminManagementService _adminManagementService;
        private readonly ILogger<SuperAdminController> _logger;

        public SuperAdminController(
            IAdminManagementService adminManagementService,
            ILogger<SuperAdminController> logger)
        {
            _adminManagementService = adminManagementService;
            _logger = logger;
        }

        // endpoint SuperAdmin tạo admin mới
        [HttpPost("admins")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang tạo admin mới: {Email}", superAdminId, dto.Email);

            var result = await _adminManagementService.CreateAdminAsync(dto);
            return result.Success
                ? CreatedAtAction(nameof(GetAdminById), new { userId = result.Data?.UserId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin lấy danh sách admins với phân trang
        [HttpGet("admins")]
        public async Task<IActionResult> GetAdmins([FromQuery] AdminQueryParameters parameters)
        {
            var superAdminId = User.GetUserIdSafe();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang lấy danh sách admins", superAdminId);

            var result = await _adminManagementService.GetAdminsPagedAsync(parameters);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin lấy chi tiết admin theo ID
        [HttpGet("admins/{userId}")]
        public async Task<IActionResult> GetAdminById(int userId)
        {
            var superAdminId = User.GetUserIdSafe();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang xem chi tiết admin {UserId}", superAdminId, userId);

            var result = await _adminManagementService.GetAdminByIdAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin xóa admin (remove Admin role)
        [HttpDelete("admins/{userId}")]
        public async Task<IActionResult> DeleteAdmin(int userId)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang xóa admin {UserId}", superAdminId, userId);

            var result = await _adminManagementService.DeleteAdminAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin reset password admin
        [HttpPut("admins/{userId}/reset-password")]
        public async Task<IActionResult> ResetAdminPassword(int userId, [FromBody] ResetAdminPasswordDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang reset password cho admin {UserId}", superAdminId, userId);

            dto.UserId = userId;
            var result = await _adminManagementService.ResetAdminPasswordAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin đổi email admin
        [HttpPut("admins/{userId}/email")]
        public async Task<IActionResult> ChangeAdminEmail(int userId, [FromBody] ChangeAdminEmailDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang đổi email cho admin {UserId}", superAdminId, userId);

            dto.UserId = userId;
            var result = await _adminManagementService.ChangeAdminEmailAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin gán role cho user
        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignRoleDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang gán role {RoleName} cho user {UserId}", superAdminId, dto.RoleName, userId);

            dto.UserId = userId;
            var result = await _adminManagementService.AssignRoleAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint SuperAdmin xóa role khỏi user
        [HttpDelete("users/{userId}/roles")]
        public async Task<IActionResult> RemoveRole(int userId, [FromBody] RemoveRoleDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang xóa role {RoleName} khỏi user {UserId}", superAdminId, dto.RoleName, userId);

            dto.UserId = userId;
            var result = await _adminManagementService.RemoveRoleAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // ═══════════════════════════════════════════════════════════════
        // ROLE & PERMISSION VIEW - Chỉ SuperAdmin (Read-only, fix cứng)
        // ═══════════════════════════════════════════════════════════════

        // GET: api/superadmin/roles - Lấy danh sách tất cả roles (read-only)
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var superAdminId = User.GetUserIdSafe();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang xem danh sách roles", superAdminId);

            var result = await _adminManagementService.GetAllRolesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/superadmin/permissions - Lấy danh sách tất cả permissions (read-only)
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var superAdminId = User.GetUserIdSafe();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang xem danh sách permissions", superAdminId);

            var result = await _adminManagementService.GetAllPermissionsAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

