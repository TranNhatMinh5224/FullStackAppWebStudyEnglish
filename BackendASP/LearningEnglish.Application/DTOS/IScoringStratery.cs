using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Strategies
{
    public interface IScoringStrategy
    {
        QuestionType SupportedType { get; }  // Loại câu hỏi mà strategy này hỗ trợ
        decimal Score(Question question, object userAnswer);  // Chấm điểm: trả về điểm số (0 đến Points của question)
    }
}