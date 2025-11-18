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
    
    // DTO cho MultipleChoice và TrueFalse: Chọn 1 đáp án
    public class SingleChoiceAnswerDto
    {
        public int QuestionId { get; set; }
        public int SelectedOptionId { get; set; }  // ID của option được chọn
    }

    // DTO cho MultipleAnswers: Chọn nhiều đáp án
    public class MultipleChoiceAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();  // Danh sách ID các option được chọn
    }

    // DTO cho FillBlank: Điền vào chỗ trống
    public class FillBlankAnswerDto
    {
        public int QuestionId { get; set; }
        public string AnswerText { get; set; } = string.Empty;  // Text điền vào
    }

    // DTO cho Matching: Ghép nối các cặp (Format: { "leftOptionId": rightOptionId, ... })
    public class MatchingAnswerDto
    {
        public int QuestionId { get; set; }
        public Dictionary<int, int> Matches { get; set; } = new();  // Key: leftOptionId, Value: rightOptionId
    }

    // DTO cho Ordering: Sắp xếp thứ tự
    public class OrderingAnswerDto
    {
        public int QuestionId { get; set; }
        public List<int> OrderedOptionIds { get; set; } = new();  // Thứ tự các option (từ trên xuống dưới)
    }
}

