using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

public class TeacherPackage
{
    public int TeacherPackageId { get; set; }

    public string PackageName { get; set; } = string.Empty;
    public PackageLevel Level { get; set; }
    public decimal Price { get; set; }

    // Duration: chọn 1 trong 2 cách; ví dụ dùng tháng:
    public int DurationMonths { get; set; } = 12;

    // Limits
    public int MaxCourses { get; set; }
    public int MaxLessons { get; set; }
    public int MaxStudents { get; set; }

    // Navigation (optional reverse)
    public List<TeacherSubscription> Subscriptions { get; set; } = new();
}
