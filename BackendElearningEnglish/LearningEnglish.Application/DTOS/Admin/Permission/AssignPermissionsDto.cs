namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO để gán permissions cho admin
/// </summary>
public class AssignPermissionsDto
{
    public int UserId { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}
