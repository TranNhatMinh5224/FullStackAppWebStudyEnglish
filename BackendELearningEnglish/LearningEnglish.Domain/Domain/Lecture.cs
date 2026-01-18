
using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Domain.Entities
{


    public class Lecture
    {
        public int LectureId { get; set; }
        public int ModuleId { get; set; }




        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }


        public string Title { get; set; } = string.Empty;
        public LectureType Type { get; set; } = LectureType.Content;
        public string? MarkdownContent { get; set; }


        public string? MediaKey { get; set; }
        public string? MediaType { get; set; }
        public long? MediaSize { get; set; }
        public int? Duration { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // tree
        public int? ParentLectureId { get; set; }
        public Lecture? Parent { get; set; }
        public List<Lecture> Children { get; set; } = new();

        public Module? Module { get; set; }
    }
}
