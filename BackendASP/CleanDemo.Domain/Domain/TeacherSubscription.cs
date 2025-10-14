namespace CleanDemo.Domain.Entities;

public class TeacherSubscription
{
    public int TeacherSubscriptionId { get; set; }
    public int TeacherId { get; set; }
    public int TeacherPackageId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public User Teacher { get; set; } = null!;
    public TeacherPackage TeacherPackage { get; set; } = null!;
}
