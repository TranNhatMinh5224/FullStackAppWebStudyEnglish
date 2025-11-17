using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface.Strategies;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public class OrderingScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.Ordering;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            if (userAnswer == null) return 0m;

            if (userAnswer is List<int> userOrder)
            {
                var correctOrder = ScoringHelper.ParseCorrectOrder(question.CorrectAnswersJson);
                if (correctOrder != null && userOrder.SequenceEqual(correctOrder))
                    return question.Points;  // Đúng thứ tự: full điểm
            }
            return 0;  // Sai: 0 điểm
        }
    }
}