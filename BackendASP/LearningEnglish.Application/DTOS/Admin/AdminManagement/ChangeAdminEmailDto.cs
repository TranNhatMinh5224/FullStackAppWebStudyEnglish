namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO đổi email admin
/// </summary>
public class ChangeAdminEmailDto
{
    public int UserId { get; set; }
    public string NewEmail { get; set; } = string.Empty;
}
