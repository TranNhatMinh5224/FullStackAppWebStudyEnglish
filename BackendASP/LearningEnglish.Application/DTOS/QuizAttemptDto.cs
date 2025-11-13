using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // Request để bắt đầu attempt mới
    public class StartQuizAttemptRequestDto
    {
        public int QuizId { get; set; }
        public bool ResumeIfActive { get; set; } = true; // true: trả attempt InProgress hiện tại nếu có
    }

    // Response khi start attempt - bao gồm bộ đề đã shuffle
    public class StartQuizAttemptResponseDto
    {
        public QuizAttemptDto Attempt { get; set; } = null!;
        public QuizContentDto QuizContent { get; set; } = null!; // Bộ đề đã shuffle
    }

    // Bộ đề quiz đã shuffle (structure + questions + answers)
    public class QuizContentDto
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public decimal? PassingPercentage { get; set; }
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleAnswers { get; set; }
        
        // Structure tùy theo cấu hình quiz
        public List<QuizSectionContentDto>? Sections { get; set; } // Nếu có sections
        public List<QuizGroupContentDto>? Groups { get; set; }     // Nếu có groups (không có sections)
        public List<QuestionContentDto>? Questions { get; set; }   // Flat structure (không có sections/groups)
    }

    public class QuizSectionContentDto
    {
        public int SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public List<QuizGroupContentDto> Groups { get; set; } = new();
    }

    public class QuizGroupContentDto
    {
        public int GroupId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public List<QuestionContentDto> Questions { get; set; } = new();
    }

    public class QuestionContentDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public TypeQuestion Type { get; set; }
        public decimal Points { get; set; }
        public int OrderIndex { get; set; }
        public string? Hint { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public List<AnswerOptionContentDto> AnswerOptions { get; set; } = new(); // Đã shuffle nếu cần
    }

    public class AnswerOptionContentDto
    {
        public int AnswerOptionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        // KHÔNG bao gồm IsCorrect - user không được biết đáp án đúng
    }

    // Dữ liệu core attempt
    public class QuizAttemptDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int UserId { get; set; }
        public int AttemptNumber { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public StatusQuizAttempt Status { get; set; }
        public int TimeSpentSeconds { get; set; }
        public List<AttemptAnswerDto> Answers { get; set; } = new();
    }

    // Answer cá nhân trong attempt
    public class AttemptAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new(); // Hỗ trợ single/multi-select
        public string? AnswerText { get; set; } // Essay/Fill blank
        public bool? IsCorrect { get; set; } // Chỉ populated nếu được phép xem
        public decimal? PointsAwarded { get; set; }
    }

    // DTO để cập nhật câu trả lời theo thời gian thực
    public class UpdateUserAnswerDto
    {
        public int QuestionId { get; set; }
        public int? SelectedAnswerId { get; set; }
        public List<int>? SelectedAnswerIds { get; set; }
        public string? TextAnswer { get; set; }
        public int? TimeSpentSeconds { get; set; }
    }

    // Submit final answers cho attempt
    public class SubmitQuizAttemptRequestDto
    {
        public int AttemptId { get; set; }
        public List<SubmittedAnswerDto> Answers { get; set; } = new();
        public bool ForceFinish { get; set; } = false; // true: finalize ngay cả khi còn thời gian
    }

    public class SubmittedAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public string? AnswerText { get; set; }
    }

    // Result sau khi chấm điểm
    public class QuizAttemptResultDto
    {
        public int AttemptId { get; set; }
        public decimal Score { get; set; }
        public decimal MaxScore { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        public StatusQuizAttempt Status { get; set; }
        public List<AttemptAnswerDto> Answers { get; set; } = new();
        public string? TeacherFeedback { get; set; }
    }
}
