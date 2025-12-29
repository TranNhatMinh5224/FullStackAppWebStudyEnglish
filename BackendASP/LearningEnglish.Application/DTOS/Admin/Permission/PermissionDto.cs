namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO hiển thị thông tin Permission
/// </summary>
public class PermissionDto
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
