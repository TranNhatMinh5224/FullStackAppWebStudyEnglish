namespace CleanDemo.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;
    
    // Navigation Properties
    public List<User> Users { get; set; } = new List<User>();
}
