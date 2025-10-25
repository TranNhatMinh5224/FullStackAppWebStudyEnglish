namespace CleanDemo.Application.DTOs
{
    public class CourseProgressDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public decimal ProgressPercentage { get; set; } // 0.00 - 100.00
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<LessonProgressDto> LessonProgresses { get; set; } = new();
    }

    public class LessonProgressDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public double Completion { get; set; } // 0-100
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
