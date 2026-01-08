namespace LearningEnglish.Application.DTOs;

// DTO cho thông tin chi tiết học sinh trong một course cụ thể
public class StudentDetailInCourseDto
{
    // Thông tin cơ bản của học sinh
    public int UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public bool IsMale { get; set; }
    public string? AvatarUrl { get; set; }

    // Thông tin về course enrollment
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; } // Ngày tham gia course
    
    // Tiến độ học tập trong course này
    public CourseProgressDetailDto? Progress { get; set; }
}

// DTO chi tiết về tiến độ hoàn thành course
public class CourseProgressDetailDto
{
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public decimal ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime LastUpdated { get; set; }
    public string ProgressDisplay { get; set; } = string.Empty; // "5/10 (50.0%)"
}
