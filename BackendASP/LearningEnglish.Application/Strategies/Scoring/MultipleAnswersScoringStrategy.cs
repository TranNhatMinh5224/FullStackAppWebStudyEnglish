using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Common.Helpers;

namespace LearningEnglish.Application.Strategies.Scoring
{
    // chấm điểm cho câu hỏi nhiều đáp án (Multiple Answers)
    public class MultipleAnswersScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.MultipleAnswers;

        public decimal CalculateScore(Question question, object? userAnswer)
        {
            if (userAnswer == null)
            {
                return 0m;
            }

            // Tự normalize answer về List<int>
            var selectedOptionIds = AnswerNormalizer.NormalizeToListInt(userAnswer);
            if (selectedOptionIds == null || selectedOptionIds.Count == 0)
            {
                return 0m;
            }

            var correctOptionIds = question.Options
                .Where(o => o.IsCorrect)
                .Select(o => o.AnswerOptionId)
                .ToList();

            // Kiểm tra nếu số đáp án chọn không bằng số đáp án đúng thì trả về 0 điểm
            if (selectedOptionIds.Count != correctOptionIds.Count)
            {
                return 0m;
            }

            // Kiểm tra tất cả đáp án đúng có trong danh sách đáp án chọn không
            if (selectedOptionIds.All(x => correctOptionIds.Contains(x)))
            {
                return question.Points;
            }

            return 0m;
        }
    }
}