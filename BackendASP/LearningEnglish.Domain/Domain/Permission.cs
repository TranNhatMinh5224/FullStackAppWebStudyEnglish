namespace LearningEnglish.Domain.Entities;

public class Permission
{
    public int PermissionId { get; set; }
    public string Name { get; set; } = string.Empty;            // e.g., "Course.Create", "User.Edit"
    public string DisplayName { get; set; } = string.Empty;     // e.g., "Tạo khóa học", "Chỉnh sửa người dùng"
    public string? Description { get; set; }                    // e.g., "Cho phép tạo khóa học mới"
    public string Module { get; set; } = string.Empty;          // e.g., "Course", "User", "Assessment"
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public List<RolePermission> RolePermissions { get; set; } = new();
}
