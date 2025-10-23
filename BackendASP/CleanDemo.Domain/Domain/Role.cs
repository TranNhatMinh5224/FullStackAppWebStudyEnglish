
namespace CleanDemo.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Many-to-many
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<User> Users { get; set; } = new List<User>();
}
