using CleanDemo.Domain.Enums;

namespace CleanDemo.Domain.Entities;

public class Course
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Img { get; set; }
    public CourseType Type { get; set; } = CourseType.System;
    public decimal? Price { get; set; }
    public int? TeacherId { get; set; }
    public string? ClassCode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public User? Teacher { get; set; }
    public List<Lesson> Lessons { get; set; } = new();
    public List<UserCourse> UserCourses { get; set; } = new();
    public bool IsFree()
    {
        return Price == null || Price == 0;
    }

}


