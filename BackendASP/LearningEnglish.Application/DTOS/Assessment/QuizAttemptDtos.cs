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

        // Chi tiết từng câu hỏi với đáp án (khi ShowAnswersAfterSubmit = true)
        public List<QuestionReviewDto> Questions { get; set; } = new();

        public DateTime SubmittedAt { get; set; }
        public int TimeSpentSeconds { get; set; }
    }

    // DTO cho đáp án đúng của câu hỏi
    public class CorrectAnswerDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> CorrectOptions { get; set; } = new();  // Text của đáp án đúng
        public object? UserAnswer { get; set; }  // Câu trả lời của user (single value, List<string>, hoặc matching pairs)
    }

    // DTO cho attempt + questions đã shuffle (dùng khi start)
    public class QuizAttemptWithQuestionsDto : QuizAttemptDto
    {
        // Danh sách sections/groups/questions đã shuffle
        public List<AttemptQuizSectionDto> QuizSections { get; set; } = new();
    }

    // DTO cho section trong quiz (dùng cho attempt) - Merged structure với ItemIndex
    public class AttemptQuizSectionDto
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;  // Tên section (Listening, Reading)
        public List<QuizItemDto> Items { get; set; } = new();  // Merged Groups + Questions
    }

    
    public class QuizItemDto
    {
        public string ItemType { get; set; } = string.Empty;  // "Group" hoặc "Question"
        public int ItemIndex { get; set; }  // Thứ tự hiển thị (0, 1, 2...)

        // === GROUP PROPERTIES (null nếu ItemType = "Question") ===
        public int? GroupId { get; set; }
        public string? Name { get; set; }  // Tên group (Part 1, Part 2)
        public string? Title { get; set; }  // Tiêu đề group
        public string? Description { get; set; }  // Mô tả group
        public string? ImgUrl { get; set; }
        public string? VideoUrl { get; set; }
        public string? ImgType { get; set; }  // Loại file ảnh
        public string? VideoType { get; set; }  // Loại file video
        public int? VideoDuration { get; set; }  // Độ dài video (giây)
        public float? SumScore { get; set; }  // Tổng điểm group
        public List<QuestionDto>? Questions { get; set; }  // Questions trong group

        // === QUESTION PROPERTIES (null nếu ItemType = "Group") ===
        public int? QuestionId { get; set; }
        public string? QuestionText { get; set; }
        public string? MediaUrl { get; set; }
        public QuestionType? Type { get; set; }
        public decimal? Points { get; set; }
        public bool? IsAnswered { get; set; }
        public decimal? CurrentScore { get; set; }
        public object? UserAnswer { get; set; }
        public List<AnswerOptionDto>? Options { get; set; }
    }

    // DTO cho câu hỏi (không include đáp án đúng)
    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
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
        public string? MediaUrl { get; set; }
    }

    // DTO cho request update score (input từ frontend)
    public class UpdateScoreRequestDto
    {
        public int AttemptId { get; set; }  // Thêm AttemptId để xác định attempt
        public int QuestionId { get; set; }
        public object? UserAnswer { get; set; }  // Câu trả lời (List<int> cho MCQ, string cho Essay)
    }

    public class ResultQuizDto
    {
        bool IsPassed { get; set; }
        int? TotalScore { get; set; }
        int? TotalCorrectAnswers { get; set; }
        int? TotalQuestions { get; set; }
        decimal Percentage { get; set; }
    }

    // DTO cho teacher xem chi tiết bài làm của học sinh
    public class QuizAttemptDetailDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public string QuizTitle { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int AttemptNumber { get; set; }
        
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public QuizAttemptStatus Status { get; set; }
        
        public int TimeSpentSeconds { get; set; }
        public decimal TotalScore { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        
        // Chi tiết từng câu hỏi với đáp án
        public List<QuestionReviewDto> Questions { get; set; } = new();
    }

    // DTO cho từng câu hỏi khi review (teacher xem bài làm)
    public class QuestionReviewDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public QuestionType Type { get; set; }
        public decimal Points { get; set; }
        public decimal Score { get; set; }
        public bool IsCorrect { get; set; }
        
        // Đáp án học sinh chọn
        public object? UserAnswer { get; set; }
        public string? UserAnswerText { get; set; }  // Human-readable format
        
        // Đáp án đúng
        public object? CorrectAnswer { get; set; }
        public string? CorrectAnswerText { get; set; }  // Human-readable format
        
        // Danh sách options (cho MCQ, Matching)
        public List<AnswerOptionReviewDto> Options { get; set; } = new();
    }

    // DTO cho option khi review (bao gồm IsCorrect)
    public class AnswerOptionReviewDto
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsSelected { get; set; }  // Học sinh có chọn option này không
    }

    // DTO cho response check active attempt
    public class ActiveAttemptDto
    {
        public bool HasActiveAttempt { get; set; }
        public int? AttemptId { get; set; }
        public int? QuizId { get; set; }
        public string? QuizTitle { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EndTime { get; set; }  // StartedAt + Duration
        public int? TimeRemainingSeconds { get; set; }  // Thời gian còn lại (nếu có duration)
    }
}