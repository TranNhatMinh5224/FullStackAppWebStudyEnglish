using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

public class TeacherPackage
{
    public int TeacherPackageId { get; set; }
    public int TeacherId { get; set; }
    public PackageLevel Level { get; set; }
    public DateTime ActivatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiredAt { get; set; }

    public int MaxCourses { get; set; }
    public int MaxLessons { get; set; }
    public int MaxStudents { get; set; }

    public bool IsActive => DateTime.UtcNow < ExpiredAt;
    
    // Navigation Properties
    public User? Teacher { get; set; }
}
