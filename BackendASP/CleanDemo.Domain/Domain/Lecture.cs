
using CleanDemo.Domain.Enums;
namespace CleanDemo.Domain.Entities
{


    public class Lecture
    {
        public int LectureId { get; set; }
        public int ModuleId { get; set; }




        public int OrderIndex { get; set; }
        public string? NumberingLabel { get; set; }


        public string Title { get; set; } = string.Empty;
        public TypeLecture Type { get; set; } = TypeLecture.Content;
        public string? MarkdownContent { get; set; }
        public string RenderedHtml { get; set; } = string.Empty;



        // audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // tree
        public int? ParentLectureId { get; set; }
        public Lecture? Parent { get; set; }
        public List<Lecture> Children { get; set; } = new();

        public Module? Module { get; set; }

        public List<Assessment> Assessments { get; set; } = new();
    }
}
