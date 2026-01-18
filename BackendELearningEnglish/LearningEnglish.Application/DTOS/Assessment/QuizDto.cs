using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{

    public class QuizDto
    {
        public int QuizId { get; set; }
        public int AssessmentId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; }

        public QuizType Type { get; set; }
        public QuizStatus Status { get; set; }
        public int TotalQuestions { get; set; }
        public int? PassingScore { get; set; }
        public decimal TotalPossibleScore { get; set; }


        // Han thoi gian lam bai thi
        public int? Duration { get; set; } // Thoi gian lam bai (phut)
        public DateTime? AvailableFrom { get; set; } // Thoi gian bat dau co the lam bai


        // Hien thi cau tra loi sau khi nop bai
        public bool? ShowAnswersAfterSubmit { get; set; } = true; // Hien dap an sau khi nop bai

        public bool? ShowScoreImmediately { get; set; } = true; // Hien thi diem so


        // Xao tron cau hoi
        public bool? ShuffleQuestions { get; set; } = true;
        public bool? ShuffleAnswers { get; set; } = true;

        // Practice settings (for vocab exercises, grammar practice)
        public int? MaxAttempts { get; set; } // Số lần làm tối đa (null = không giới hạn)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;




    }
    public class QuizCreateDto
    {
        public int AssessmentId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Instructions { get; set; }

        public QuizType Type { get; set; } = QuizType.Practice;
        public QuizStatus Status { get; set; } = QuizStatus.Open;
        public int TotalQuestions { get; set; }
        public int? PassingScore { get; set; }
        public decimal TotalPossibleScore { get; set; } // Tổng điểm tối đa của bài quiz (giáo viên nhập)

        // Thời gian làm bài
        public int? Duration { get; set; }
        public DateTime? AvailableFrom { get; set; }

        // Hiển thị kết quả
        public bool? ShowAnswersAfterSubmit { get; set; } = true;
        public bool? ShowScoreImmediately { get; set; } = true;

        // Xáo trộn
        public bool? ShuffleQuestions { get; set; } = true;
        public bool? ShuffleAnswers { get; set; } = true;

        // Cài đặt practice
        public int? MaxAttempts { get; set; } // Số lần làm tối đa (null = không giới hạn)

    }
    public class QuizUpdateDto : QuizCreateDto
    {
    }



}


