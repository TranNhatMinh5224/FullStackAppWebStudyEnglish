
namespace LearningEnglish.Domain.Entities;

public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;

   
}
