namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO hiển thị permissions của user
/// </summary>
public class UserPermissionsDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<PermissionWithAssignmentDto> Permissions { get; set; } = new();
}

/// <summary>
/// Permission kèm thông tin gán
/// </summary>
public class PermissionWithAssignmentDto
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
