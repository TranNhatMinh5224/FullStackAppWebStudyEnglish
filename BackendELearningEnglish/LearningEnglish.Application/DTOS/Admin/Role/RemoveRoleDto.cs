namespace LearningEnglish.Application.DTOs.Admin;


// DTO xóa role khỏi user

public class RemoveRoleDto
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
