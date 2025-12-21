namespace LearningEnglish.Application.DTOs
{
    // DTO for module pronunciation summary/statistics (simplified)
    public class ModulePronunciationSummaryDto
    {
        // Module info
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public int TotalFlashCards { get; set; }

        // Progress
        public int TotalPracticed { get; set; }              // Số từ đã luyện
        public int MasteredCount { get; set; }               // Số từ đã thuộc
        public double OverallProgress { get; set; }          // % hoàn thành (0-100)

        // Score - THÔNG TIN QUAN TRỌNG NHẤT
        public double AverageScore { get; set; }             // Điểm trung bình (0-100)
        
        // Last practice
        public DateTime? LastPracticeDate { get; set; }      // Lần luyện gần nhất

        // Status & Feedback
        public string Status { get; set; } = string.Empty;   // Not Started | In Progress | Completed | Mastered
        public string Message { get; set; } = string.Empty;  // Nhận xét/Lời khuyên
        public string Grade { get; set; } = string.Empty;    // A+ | A | B | C | D | F
    }
}
