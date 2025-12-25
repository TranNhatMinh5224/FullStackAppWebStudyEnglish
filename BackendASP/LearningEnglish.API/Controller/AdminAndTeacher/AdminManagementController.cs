using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.API.Controller.Admin;

[ApiController]
[Route("api/admin/admins")]
[Authorize(Roles = "SuperAdmin")]
public class AdminManagementController : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService;

    public AdminManagementController(IAdminManagementService adminManagementService)
    {
        _adminManagementService = adminManagementService;
    }

    /// <summary>
    /// Tạo admin mới (user + Admin role + permissions)
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
    {
        var result = await _adminManagementService.CreateAdminAsync(dto);
        return result.Success ? StatusCode(result.StatusCode, result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Lấy danh sách admins với phân trang
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAdmins([FromQuery] AdminQueryParameters parameters)
    {
        var result = await _adminManagementService.GetAdminsPagedAsync(parameters);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Update permissions của admin (replace toàn bộ)
    /// </summary>
    [HttpPut("{userId}/permissions")]
    public async Task<IActionResult> UpdateAdminPermissions(int userId, [FromBody] List<int> permissionIds)
    {
        var dto = new UpdateAdminPermissionsDto
        {
            UserId = userId,
            PermissionIds = permissionIds
        };
        var result = await _adminManagementService.UpdateAdminPermissionsAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Xóa admin (remove Admin role + permissions)
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteAdmin(int userId)
    {
        var result = await _adminManagementService.DeleteAdminAsync(userId);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Reset password admin
    /// </summary>
    [HttpPost("{userId}/reset-password")]
    public async Task<IActionResult> ResetAdminPassword(int userId, [FromBody] ResetAdminPasswordDto dto)
    {
        dto.UserId = userId;
        var result = await _adminManagementService.ResetAdminPasswordAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Đổi email admin
    /// </summary>
    [HttpPut("{userId}/email")]
    public async Task<IActionResult> ChangeAdminEmail(int userId, [FromBody] ChangeAdminEmailDto dto)
    {
        dto.UserId = userId;
        var result = await _adminManagementService.ChangeAdminEmailAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Gán role cho user (Admin/Teacher/Student)
    /// </summary>
    [HttpPost("roles/assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = await _adminManagementService.AssignRoleAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Xóa role khỏi user
    /// </summary>
    [HttpDelete("roles/remove")]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleDto dto)
    {
        var result = await _adminManagementService.RemoveRoleAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
