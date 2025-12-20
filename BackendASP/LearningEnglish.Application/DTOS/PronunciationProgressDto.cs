namespace LearningEnglish.Application.DTOs
{
    // DTO for PronunciationProgress response (simplified)
    public class PronunciationProgressDto
    {
        public int PronunciationProgressId { get; set; }
        public int UserId { get; set; }
        public int FlashCardId { get; set; }

        // FlashCard info
        public string Word { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? ImageUrl { get; set; }
        public string? AudioUrl { get; set; }

        // Simple statistics
        public double BestScore { get; set; }
        public int TotalAttempts { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        // Best assessment reference
        public int? BestAssessmentId { get; set; }
    }

    // DTO for flashcard with pronunciation progress (for module view)
    public class FlashCardWithPronunciationProgressDto
    {
        public int FlashCardId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string? Pronunciation { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }

        // Progress info
        public int TotalAttempts { get; set; }
        public double BestScore { get; set; }
        public DateTime? LastPracticedAt { get; set; }

        // Status flags
        public bool HasPracticed => TotalAttempts > 0;
        public bool IsGoodScore => BestScore >= 80;
        public bool NeedsPractice => TotalAttempts == 0 || BestScore < 70;
    }

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
