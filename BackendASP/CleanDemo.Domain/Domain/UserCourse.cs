namespace CleanDemo.Domain.Entities;

public class UserCourse
{
    public int UserCourseId { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    public User? User { get; set; }
    public Course? Course { get; set; }
}
