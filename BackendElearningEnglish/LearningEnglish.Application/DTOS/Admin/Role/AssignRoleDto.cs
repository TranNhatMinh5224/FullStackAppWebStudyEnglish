namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO gán role cho user
/// </summary>
public class AssignRoleDto
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty; // "Admin", "Teacher", "Student"
}
