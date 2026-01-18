using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // DTO for Lecture response
    public class LectureDto
    {
        public int LectureId { get; set; }
        public int ModuleId { get; set; }
        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string Title { get; set; } = string.Empty;
        public LectureType Type { get; set; }
        public string TypeName => Type.ToString();
        public string? MarkdownContent { get; set; }

        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public long? MediaSize { get; set; }
        public int? Duration { get; set; }

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
        public string? MediaUrl { get; set; }
        public LectureType Type { get; set; }
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
        public LectureType Type { get; set; } = LectureType.Content;
        public string? MarkdownContent { get; set; }
        public int? ParentLectureId { get; set; }

        // Media handling
        public string? MediaTempKey { get; set; }  // Temp key from MinIO upload
        public string? MediaType { get; set; }
        public long? MediaSize { get; set; }
        public int? Duration { get; set; }
    }

    // DTO for updating existing lecture
    public class UpdateLectureDto
    {
        public int? OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string? Title { get; set; }
        public LectureType? Type { get; set; }
        public string? MarkdownContent { get; set; }
        public int? ParentLectureId { get; set; }

        // Media handling
        public string? MediaTempKey { get; set; }  // Temp key from MinIO upload
        public string? MediaType { get; set; }
        public long? MediaSize { get; set; }
        public int? Duration { get; set; }
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

    // DTO for bulk create lectures with parent-child hierarchy
    public class BulkCreateLecturesDto
    {
        public int ModuleId { get; set; }
        public List<LectureNodeDto> Lectures { get; set; } = new();
    }

    // DTO for individual lecture node in bulk create
    public class LectureNodeDto
    {
        public string TempId { get; set; } = string.Empty; // Temporary ID for referencing (e.g., "temp-1", "temp-2")
        public string? ParentTempId { get; set; }  // Reference to parent's TempId
        
        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }
        public string Title { get; set; } = string.Empty;
        public LectureType Type { get; set; } = LectureType.Content;
        public string? MarkdownContent { get; set; }

        // Media handling
        public string? MediaTempKey { get; set; }
        public string? MediaType { get; set; }
        public long? MediaSize { get; set; }
        public int? Duration { get; set; }
    }

    // Response DTO for bulk create
    public class BulkCreateLecturesResponseDto
    {
        public int TotalCreated { get; set; }
        public Dictionary<string, LectureDto> CreatedLectures { get; set; } = new(); // TempId → LectureDto
        public List<string> Errors { get; set; } = new();
    }
}
