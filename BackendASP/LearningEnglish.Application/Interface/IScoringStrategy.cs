using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
namespace LearningEnglish.Application.Interface.Strategies
{
    public interface IScoringStrategy
    {
        QuestionType Type { get; }
        decimal CalculateScore(Question question, object? userAnswer);
    }
}
