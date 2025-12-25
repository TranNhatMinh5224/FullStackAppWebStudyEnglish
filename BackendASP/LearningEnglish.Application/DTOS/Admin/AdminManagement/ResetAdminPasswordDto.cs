namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO reset password admin
/// </summary>
public class ResetAdminPasswordDto
{
    public int UserId { get; set; }
    public string NewPassword { get; set; } = string.Empty;
}
