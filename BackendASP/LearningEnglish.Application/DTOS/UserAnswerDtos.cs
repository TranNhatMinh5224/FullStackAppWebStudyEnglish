using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.DTOs
{
    // Base DTO cho update answer
    public class UpdateAnswerRequestDto
    {
        public int QuestionId { get; set; }
        public object? UserAnswer { get; set; }  // Giữ lại để backward compatibility
    }

    // DTOs riêng cho từng loại câu hỏi (Type-safe)
    
    /// <summary>
    /// DTO cho MultipleChoice và TrueFalse: Chọn 1 đáp án
    /// </summary>
    public class SingleChoiceAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }  // ID của option được chọn
    }

    /// <summary>
    /// DTO cho MultipleAnswers: Chọn nhiều đáp án
    /// </summary>
    public class MultipleChoiceAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();  // Danh sách ID các option được chọn
    }

    /// <summary>
    /// DTO cho FillBlank: Điền vào chỗ trống
    /// </summary>
    public class FillBlankAnswerDto
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;  // Text điền vào
    }

    /// <summary>
    /// DTO cho Matching: Ghép nối các cặp
    /// Format: { "leftOptionId": rightOptionId, ... }
    /// </summary>
    public class MatchingAnswerDto
    {
        public int QuestionId { get; set; }
        public Dictionary<int, int> Matches { get; set; } = new();  // Key: leftOptionId, Value: rightOptionId
    }

    /// <summary>
    /// DTO cho Ordering: Sắp xếp thứ tự
    /// </summary>
    public class OrderingAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> OrderedOptionIds { get; set; } = new();  // Thứ tự các option (từ trên xuống dưới)
    }
}

