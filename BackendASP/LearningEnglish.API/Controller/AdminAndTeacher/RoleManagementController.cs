using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs.Admin;

namespace LearningEnglish.API.Controller.AdminAndTeacher;

[ApiController]
[Route("api/admin/roles")]
[Authorize(Roles = "SuperAdmin")]
public class RoleManagementController : ControllerBase
{
    private readonly IAdminManagementService _adminManagementService;

    public RoleManagementController(IAdminManagementService adminManagementService)
    {
        _adminManagementService = adminManagementService;
    }

    /// <summary>
    /// Gán role cho user (Admin/Teacher/Student)
    /// </summary>
    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
    {
        var result = await _adminManagementService.AssignRoleAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Xóa role khỏi user
    /// </summary>
    [HttpDelete("remove")]
    public async Task<IActionResult> RemoveRole([FromBody] RemoveRoleDto dto)
    {
        var result = await _adminManagementService.RemoveRoleAsync(dto);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
