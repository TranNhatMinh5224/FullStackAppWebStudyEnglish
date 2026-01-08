namespace LearningEnglish.Application.DTOs

{
    public class LessonDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public int OrderIndex { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
    }

    // DTO for lesson with progress info for users
    public class LessonWithProgressDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
        public int CourseId { get; set; }

        // ✅ Progress information
        public float CompletionPercentage { get; set; } = 0; // 0-100%
        public bool IsCompleted { get; set; } = false;
        public int CompletedModules { get; set; } = 0;
        public int TotalModules { get; set; } = 0;
        public float VideoProgressPercentage { get; set; } = 0;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class AdminCreateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }
    public class TeacherCreateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CourseId { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }

    public class UpdateLessonDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? OrderIndex { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }

    public class DeleteLessonDto
    {
        public int LessonId { get; set; }
    }
}
