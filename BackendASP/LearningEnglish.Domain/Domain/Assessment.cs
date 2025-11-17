namespace LearningEnglish.Domain.Entities
{
    // Tạo Bài Kiểm Tra (Assessment) cho Khóa Học , Lesson , Module với nhiều Quiz và Essay

    public class Assessment
    {
        public int AssessmentId { get; set; }

        public int ModuleId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? OpenAt { get; set; }
        public DateTime? DueAt { get; set; }
        public TimeSpan? TimeLimit { get; set; }
        public bool IsPublished { get; set; } = true;

        public decimal TotalPoints { get; set; }
        public int PassingScore { get; set; }

        // Navigation Properties
        public Module? Module { get; set; }
        public List<Essay> Essays { get; set; } = new List<Essay>();
        public List<Quiz> Quizzes { get; set; } = new List<Quiz>();
        public List<EssaySubmission> EssaySubmissions { get; set; } = new();
        public List<PronunciationAssessment> PronunciationAssessments { get; set; } = new();
    }


}
