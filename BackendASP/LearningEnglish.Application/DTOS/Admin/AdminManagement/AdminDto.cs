namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO hiển thị thông tin admin
/// </summary>
public class AdminDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<PermissionDto> Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
