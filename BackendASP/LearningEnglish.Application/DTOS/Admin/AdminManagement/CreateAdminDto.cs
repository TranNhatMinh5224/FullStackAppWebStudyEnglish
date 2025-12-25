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
    public List<int> PermissionIds { get; set; } = new(); // Content: [1,2,3], Finance: [4,5,6,7]
}
