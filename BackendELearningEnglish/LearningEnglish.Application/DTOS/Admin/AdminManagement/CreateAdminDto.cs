namespace LearningEnglish.Application.DTOs.Admin;

/// <summary>
/// DTO tạo admin mới
/// </summary>
public class CreateAdminDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int RoleId { get; set; } // 2 = ContentAdmin, 3 = FinanceAdmin
}
