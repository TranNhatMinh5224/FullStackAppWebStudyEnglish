using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public class TrueFalseScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.TrueFalse;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            if (userAnswer == null) return 0m;

            // User answer có thể là int (optionId) hoặc bool
            int? selectedOptionId = null;
            
            // Thử normalize về int trước
            selectedOptionId = AnswerNormalizer.NormalizeToInt(userAnswer);
            
            // Nếu không được, thử parse từ bool
            if (!selectedOptionId.HasValue && userAnswer is bool boolAnswer)
            {
                var trueOption = question.Options.FirstOrDefault(o => o.Text?.ToLower() == "true");
                var falseOption = question.Options.FirstOrDefault(o => o.Text?.ToLower() == "false");
                selectedOptionId = boolAnswer ? trueOption?.AnswerOptionId : falseOption?.AnswerOptionId;
            }

            if (selectedOptionId.HasValue)
            {
                var selectedOption = question.Options.FirstOrDefault(o => o.AnswerOptionId == selectedOptionId.Value);
                if (selectedOption != null && selectedOption.IsCorrect)
                    return question.Points;  // Đúng: full điểm
            }

            return 0;  // Sai: 0 điểm
        }
    }
}