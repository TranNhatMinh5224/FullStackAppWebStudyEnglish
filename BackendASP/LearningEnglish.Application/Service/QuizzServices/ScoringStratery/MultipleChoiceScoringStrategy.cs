using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public class MultipleChoiceScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.MultipleChoice;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            if (userAnswer == null) return 0m;

            // Tự normalize answer về int
            int? selectedOptionId = AnswerNormalizer.NormalizeToInt(userAnswer);
            if (!selectedOptionId.HasValue) return 0m;

            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            if (correctOption != null && correctOption.AnswerOptionId == selectedOptionId.Value)
            {
                return question.Points; 
            }
            return 0m;
        } 
    }
}
