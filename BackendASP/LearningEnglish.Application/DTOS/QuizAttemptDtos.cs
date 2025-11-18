using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    public class QuizAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public int AttemptNumber { get; set; }  // Lần làm thứ mấy

        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public QuizAttemptStatus Status { get; set; }

        public int TimeSpentSeconds { get; set; }  // Thời gian làm bài
        public decimal TotalScore { get; set; }    // Điểm tổng

        // JSON điểm từng câu (optional, nếu cần show chi tiết)
        public string? ScoresJson { get; set; }

        // Thêm nếu cần: EndTime tính từ StartedAt + Duration (không lưu DB)
        public DateTime? EndTime { get; set; }
    }

    // DTO cho kết quả chi tiết sau submit (điểm, đáp án đúng)
    public class QuizAttemptResultDto
    {
        public int AttemptId { get; set; }
        public decimal TotalScore { get; set; }
        public decimal Percentage { get; set; }  // % điểm so với max possible
        public bool IsPassed { get; set; }       // TotalScore >= PassingScore

        // Điểm từng câu (parse từ ScoresJson)
        public Dictionary<int, decimal> ScoresByQuestion { get; set; } = new();

        // Danh sách đáp án đúng (không show user answers)
        public List<CorrectAnswerDto> CorrectAnswers { get; set; } = new();

        public DateTime SubmittedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
    }

    // DTO cho đáp án đúng của câu hỏi
    public class CorrectAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> CorrectOptions { get; set; } = new();  // Text của đáp án đúng
    }

    // DTO cho attempt + questions đã shuffle (dùng khi start)
    public class QuizAttemptWithQuestionsDto : QuizAttemptDto
    {
        // Danh sách sections/groups/questions đã shuffle
        public List<AttemptQuizSectionDto> QuizSections { get; set; } = new();
    }

    // DTO cho section trong quiz (dùng cho attempt)
    public class AttemptQuizSectionDto
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;  // Tên section (Listening, Reading)
        public List<AttemptQuizGroupDto> QuizGroups { get; set; } = new();
        public List<QuestionDto> Questions { get; set; } = new();  // Questions không thuộc group
    }

    // DTO cho group trong section (dùng cho attempt)
    public class AttemptQuizGroupDto
    {
        public int GroupId { get; set; }
        public string Name { get; set; } = string.Empty;  // Tên group (Part 1, Part 2)
        public List<QuestionDto> Questions { get; set; } = new();
    }

    // DTO cho câu hỏi (không include đáp án đúng)
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public QuestionType Type { get; set; }
        public decimal Points { get; set; }
        public bool IsAnswered { get; set; } = false;  // Đánh dấu đã trả lời
        public decimal? CurrentScore { get; set; }     // Điểm hiện tại (nếu đã trả lời)
        public object? UserAnswer { get; set; }         // Câu trả lời của user (để hiển thị khi resume)
        public List<AnswerOptionDto> Options { get; set; } = new();
    }

    // DTO cho đáp án (không include IsCorrect)
    public class AnswerOptionDto
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
    }

    // DTO cho request update score (input từ frontend)
    public class UpdateScoreRequestDto
    {
        public int AttemptId { get; set; }  // Thêm AttemptId để xác định attempt
        public int QuestionId { get; set; }
        public object? UserAnswer { get; set; }  // Câu trả lời (List<int> cho MCQ, string cho Essay)
    }

    // DTO cho request update answer (input từ frontend)
    // NOTE: Class này đã được move sang UserAnswerDtos.cs để tránh conflict
    // Giữ lại ở đây để backward compatibility, nhưng nên dùng từ UserAnswerDtos.cs
    // [Obsolete("Use UpdateAnswerRequestDto from UserAnswerDtos.cs")]
    // public class UpdateAnswerRequestDto
    // {
    //     public int QuestionId { get; set; }
    //     public object? UserAnswer { get; set; }
    // }

    public class ResultQuizDto
    {
        bool IsPassed { get; set; }
        int? TotalScore { get; set; }
        int? TotalCorrectAnswers { get; set; }
        int? TotalQuestions { get; set; }
        decimal Percentage { get; set; }
    }
}