using System.Collections.Generic;

namespace CleanDemo.Application.DTOs
{
    public class CreateAnswerDto
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class CreateQuestionDto
    {
        public int MiniTestId { get; set; }
        public string Text { get; set; } = string.Empty;
        public List<CreateAnswerDto> Answers { get; set; } = new();
    }

    public class AnswerDto
    {
        public int AnswerOptionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int QuestionId { get; set; }
    }

    public class QuestionDto
    {
        public int QuestionId { get; set; }
        public string Text { get; set; } = string.Empty;
        public int MiniTestId { get; set; }
        public List<AnswerDto> Answers { get; set; } = new();
    }
}
