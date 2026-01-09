using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // DTO for Module response
    public class ModuleDto
    {
        public int ModuleId { get; set; }
        public int LessonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public ModuleType ContentType { get; set; }
        public string ContentTypeName => ContentType.ToString();
        public string? ImageUrl { get; set; }
        public string? ImageType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation info
        public string? LessonTitle { get; set; }

        // Content counts
        public int LectureCount { get; set; }
        public int FlashCardCount { get; set; }
        public int AssessmentCount { get; set; }
    }

    // DTO for listing modules (lighter version)
    public class ListModuleDto
    {
        public int ModuleId { get; set; }
        public int LessonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public ModuleType ContentType { get; set; }
        public string ContentTypeName => ContentType.ToString();
        public DateTime CreatedAt { get; set; }
        
        // ✅ Progress information (populated for students)
        public bool IsCompleted { get; set; }
        public decimal ProgressPercentage { get; set; }
    }

    // DTO for creating new module
    public class CreateModuleDto
    {
        public int LessonId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public ModuleType ContentType { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }

    // DTO for updating existing module
    public class UpdateModuleDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? OrderIndex { get; set; }
        public ModuleType? ContentType { get; set; }
        public string? ImageTempKey { get; set; }
        public string? ImageType { get; set; }
    }

    // DTO for module with progress info for users
    public class ModuleWithProgressDto : ModuleDto
    {
        public bool IsCompleted { get; set; }
        public decimal ProgressPercentage { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    // DTO for reordering modules
    public class ReorderModuleDto
    {
        public int ModuleId { get; set; }
        public int NewOrderIndex { get; set; }
    }

    // DTO for bulk operations
    public class BulkModuleOperationDto
    {
        public List<int> ModuleIds { get; set; } = new();
        public string Operation { get; set; } = string.Empty; // "delete", "move", "duplicate"
        public int? TargetLessonId { get; set; } // For move/duplicate operations
    }
}
