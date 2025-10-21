namespace CleanDemo.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<UserRole> UserRoles { get; set; } = new();
    public ICollection<User> Users { get; set; } = new List<User>();
}