namespace LearningEnglish.Application.DTOs.Admin;


/// DTO trả về thông tin role

public class RoleDto
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<PermissionDto> Permissions { get; set; } = new();
    public int UserCount { get; set; }
}

