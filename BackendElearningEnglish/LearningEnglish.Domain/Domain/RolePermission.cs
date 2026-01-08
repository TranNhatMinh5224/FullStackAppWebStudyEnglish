namespace LearningEnglish.Domain.Entities;

// Join table for many-to-many relationship between Role and Permission
public class RolePermission
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
