using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Strategies.Scoring
{
    // chấm điểm cho câu hỏi ghép nối (Matching)
    public class MatchingScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.Matching;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            if (userAnswer == null) return 0m;

            // Tự normalize answer về Dictionary<int, int>
            var userMatches = AnswerNormalizer.NormalizeToDictionaryIntInt(userAnswer);
            if (userMatches == null || userMatches.Count == 0) return 0m;

            var correctMatches = ScoringHelper.ParseCorrectMatches(question.CorrectAnswersJson, question.MetadataJson, question.Options);
            if (correctMatches != null && userMatches.Count == correctMatches.Count)
            {
                foreach (var pair in userMatches)
                {
                    if (!correctMatches.TryGetValue(pair.Key, out var correctRight) || correctRight != pair.Value)
                        return 0;  // Sai cặp: 0 điểm
                }
                return question.Points;  // Tất cả đúng: full điểm
            }
            return 0;
        }
    }
}