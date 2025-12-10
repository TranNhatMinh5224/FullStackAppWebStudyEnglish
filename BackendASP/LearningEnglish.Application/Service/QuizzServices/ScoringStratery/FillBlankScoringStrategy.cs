using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public class FillBlankScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.FillBlank;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            // Tự normalize answer về string
            string userAnswerStr = AnswerNormalizer.NormalizeToString(userAnswer)
                                        .Trim()
                                        .ToLower();

            // Parse danh sách đáp án đúng từ CorrectAnswersJson
            var correctAnswersList = QuestionAnswerParser.ParseCorrectAnswers(question.CorrectAnswersJson);

            foreach (var correctAnswer in correctAnswersList)
            {
                if (userAnswerStr.Equals(correctAnswer?.Trim() ?? string.Empty, StringComparison.CurrentCultureIgnoreCase))
                {
                    return question.Points;
                }
            }

            return 0m;
        }
    }
}
