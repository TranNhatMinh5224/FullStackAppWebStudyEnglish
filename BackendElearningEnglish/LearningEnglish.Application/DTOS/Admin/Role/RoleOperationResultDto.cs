namespace LearningEnglish.Application.DTOs.Admin;


// Kết quả gán/xóa role

public class RoleOperationResultDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
