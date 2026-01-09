namespace LearningEnglish.Domain.Entities
{
    // Tạo Bài Kiểm Tra (Assessment) cho Khóa Học , Lesson , Module với nhiều Quiz và Essay

    public class Assessment
    {
        public int AssessmentId { get; set; }

        public int ModuleId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime? OpenAt { get; set; } // Thời gian mở bài kiểm tra
        public DateTime? DueAt { get; set; } // Thời gian kết thúc bài kiểm tra ví dụ : hạn cuối nộp bài  
        public TimeSpan? TimeLimit { get; set; } // Giới hạn thời gian làm bài

        public bool IsPublished { get; set; } = true; // Bài kiểm tra có được công khai hay không

        // Navigation Properties
        public Module? Module { get; set; }
        public List<Essay> Essays { get; set; } = new List<Essay>();
        public List<Quiz> Quizzes { get; set; } = new List<Quiz>();
    }


}
