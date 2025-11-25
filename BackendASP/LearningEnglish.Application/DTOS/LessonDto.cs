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
    public class ListLessonDto
    {
        public int LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
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
