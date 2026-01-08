
namespace LearningEnglish.Domain.Entities;

public class Role
{
    public int RoleId { get; set; }
    public string Name { get; set; } = string.Empty;

    
    public List<User> Users { get; set; } = new List<User>();
   
    public List<RolePermission> RolePermissions { get; set; } = new();
}
