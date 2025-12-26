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

            // Lấy danh sách admins và filter theo userId
            var parameters = new AdminQueryParameters { PageNumber = 1, PageSize = 1 };
            var result = await _adminManagementService.GetAdminsPagedAsync(parameters);
            
            if (!result.Success)
            {
                return StatusCode(result.StatusCode, result);
            }

            var admin = result.Data?.Items?.FirstOrDefault(a => a.UserId == userId);
            if (admin == null)
            {
                return NotFound(new { message = "Không tìm thấy admin" });
            }

            return Ok(new { success = true, statusCode = 200, data = admin });
        }

        // endpoint SuperAdmin cập nhật permissions của admin
        [HttpPut("admins/{userId}/permissions")]
        public async Task<IActionResult> UpdateAdminPermissions(int userId, [FromBody] UpdateAdminPermissionsDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang cập nhật permissions cho admin {UserId}", superAdminId, userId);

            // Đảm bảo userId trong DTO khớp với route
            dto.UserId = userId;
            var result = await _adminManagementService.UpdateAdminPermissionsAsync(dto);
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

        // endpoint SuperAdmin nâng cấp user thành Teacher (gán role + tạo subscription)
        // Dùng khi thanh toán thất bại hoặc cần xử lý thủ công
        [HttpPost("users/upgrade-to-teacher")]
        public async Task<IActionResult> UpgradeUserToTeacher([FromBody] UpgradeUserToTeacherDto dto)
        {
            var superAdminId = User.GetUserId();
            _logger.LogInformation("SuperAdmin {SuperAdminId} đang nâng cấp user {Email} thành Teacher với package {PackageId}", 
                superAdminId, dto.Email, dto.TeacherPackageId);

            var result = await _adminManagementService.UpgradeUserToTeacherAsync(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

