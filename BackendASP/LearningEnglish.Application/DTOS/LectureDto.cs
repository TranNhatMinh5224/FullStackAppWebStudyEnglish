using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOS
{
    // DTO for Lecture response
    public class LectureDto
    {
        public int LectureId { get; set; }
        public int ModuleId { get; set; }
        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string Title { get; set; } = string.Empty;
        public TypeLecture Type { get; set; }
        public string TypeName => Type.ToString();
        public string? MarkdownContent { get; set; }
        public string RenderedHtml { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        // Tree structure
        public int? ParentLectureId { get; set; }
        public string? ParentTitle { get; set; }
        
        // Navigation info
        public string? ModuleName { get; set; }
        
        // Child count
        public int ChildrenCount { get; set; }
        public int AssessmentCount { get; set; }
    }

    // DTO for listing lectures (lighter version)
    public class ListLectureDto
    {
        public int LectureId { get; set; }
        public int ModuleId { get; set; }
        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string Title { get; set; } = string.Empty;
        public TypeLecture Type { get; set; }
        public string TypeName => Type.ToString();
        public DateTime CreatedAt { get; set; }
        public int? ParentLectureId { get; set; }
        public int ChildrenCount { get; set; }
    }

    // DTO for creating new lecture
    public class CreateLectureDto
    {
        public int ModuleId { get; set; }
        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string Title { get; set; } = string.Empty;
        public TypeLecture Type { get; set; } = TypeLecture.Content;
        public string? MarkdownContent { get; set; }
        public int? ParentLectureId { get; set; }
    }

    // DTO for updating existing lecture
    public class UpdateLectureDto
    {
        public int? OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string? Title { get; set; }
        public TypeLecture? Type { get; set; }
        public string? MarkdownContent { get; set; }
        public int? ParentLectureId { get; set; }
    }

    // DTO for lecture with progress info for users
    public class LectureWithProgressDto : LectureDto
    {
        public bool IsCompleted { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
    }

    // DTO for reordering lectures
    public class ReorderLectureDto
    {
        public int LectureId { get; set; }
        public int NewOrderIndex { get; set; }
        public int? NewParentLectureId { get; set; }
    }

    // DTO for lecture tree structure
    public class LectureTreeDto : ListLectureDto
    {
        public List<LectureTreeDto> Children { get; set; } = new();
    }
}
