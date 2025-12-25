namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// Kết quả update permissions
/// </summary>
public class UpdateAdminPermissionsResultDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> RemovedPermissions { get; set; } = new();
    public List<string> AddedPermissions { get; set; } = new();
    public List<PermissionDto> CurrentPermissions { get; set; } = new();
}
