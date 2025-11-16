using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using System.Text.Json;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    // chấm điểm cho câu hỏi ghép nối (Matching)
    public class MatchingScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.Matching;

        public decimal CalculateScore(Question question, object userAnswer)
        {
            if (userAnswer is Dictionary<int, int> userMatches)
            {
                var correctMatches = ScoringHelper.ParseCorrectMatches(question.CorrectAnswersJson);
                if (correctMatches != null && userMatches.Count == correctMatches.Count)
                {
                    foreach (var pair in userMatches)
                    {
                        if (!correctMatches.TryGetValue(pair.Key, out var correctRight) || correctRight != pair.Value)
                            return 0;  // Sai cặp: 0 điểm
                    }
                    return question.Points;  // Tất cả đúng: full điểm
                }
            }
            return 0;
        }
    }
}