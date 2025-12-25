namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO update permissions của admin (replace toàn bộ)
/// </summary>
public class UpdateAdminPermissionsDto
{
    public int UserId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
