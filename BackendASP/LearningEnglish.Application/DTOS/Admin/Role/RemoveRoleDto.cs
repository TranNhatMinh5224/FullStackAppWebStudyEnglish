namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO xóa role khỏi user
/// </summary>
public class RemoveRoleDto
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
