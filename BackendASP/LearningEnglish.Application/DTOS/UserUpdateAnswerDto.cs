namespace LearningEnglish.Application.DTOs
{
    // DTO để update câu trả lời realtime khi user chọn/sửa đáp án
    public class UpdateAnswerDto
    {
        // ID của câu hỏi
        public int QuestionId { get; set; }

        // ID của đáp án được chọn (SingleChoice/TrueFalse)
        public int? SelectedAnswerId { get; set; }

        // Danh sách ID đáp án được chọn (MultipleChoice)
        public List<int>? SelectedAnswerIds { get; set; }

        // Text câu trả lời (Essay/FillBlank)
        public string? TextAnswer { get; set; }

        // Thời gian user dành cho câu này (seconds)
        public int? TimeSpentSeconds { get; set; }
    }
}
