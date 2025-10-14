namespace CleanDemo.Domain.Entities;

public class UserRole
{
    public int UsersUserId { get; set; }
    public int RolesRoleId { get; set; }

    // Navigation properties (tùy chọn, để truy cập User/Role)
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}