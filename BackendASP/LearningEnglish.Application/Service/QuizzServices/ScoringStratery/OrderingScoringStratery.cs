using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public class OrderingScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.Ordering;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            if (userAnswer == null) return 0m;

            // Tự normalize answer về List<int>
            var userOrder = AnswerNormalizer.NormalizeToListInt(userAnswer);
            if (userOrder == null || userOrder.Count == 0) return 0m;

            var correctOrder = ScoringHelper.ParseCorrectOrder(question.CorrectAnswersJson, question.Options);
            if (correctOrder != null && userOrder.SequenceEqual(correctOrder))
                return question.Points;  // Đúng thứ tự: full điểm

            return 0;  // Sai: 0 điểm
        }
    }
}