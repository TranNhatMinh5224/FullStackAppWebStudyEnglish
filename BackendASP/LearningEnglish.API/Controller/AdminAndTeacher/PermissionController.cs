using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;

namespace LearningEnglish.API.Controller.Admin;

[ApiController]
[Route("api/admin/permissions")]
[Authorize(Roles = "SuperAdmin")]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Lấy tất cả permissions
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Lấy permissions của user
    /// </summary>
    [HttpGet("users/{userId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetUserPermissions(int userId)
    {
        var result = await _permissionService.GetUserPermissionsAsync(userId);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
