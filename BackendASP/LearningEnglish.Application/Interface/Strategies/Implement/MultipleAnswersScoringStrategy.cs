using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Strategies
{
    public class MultipleAnswersScoringStrategy : IScoringStrategy
    {
        public ScoringResultDto ScoreAnswer(Question question, QuizUserAnswer userAnswer)
        {
            var result = new ScoringResultDto
            {
                IsCorrect = false,
                PointsEarned = 0,
                Percentage = 0,
                Feedback = "Not answered."
            };

          
            var correctIds = question.Options?
                .Where(o => o.IsCorrect)
                .Select(o => o.AnswerOptionId)
                .ToList() ?? new List<int>();

            // Nếu câu hỏi không có đáp án đúng
            if (!correctIds.Any())
            {
                result.Feedback = "Question has no correct answers configured.";
                return result;
            }

            // Lưu đáp án đúng để trả về cho user
            result.CorrectAnswerIds = correctIds;

            var selectedIds = userAnswer.SelectedOptions?
                .Select(o => o.AnswerOptionId)
                .ToList() ?? new List<int>();

            if (!selectedIds.Any())
            {
                result.Feedback = "Not answered.";
                return result;
            }

           
            int correctCounted = selectedIds.Count(id => correctIds.Contains(id));
            int incorrectCounted = selectedIds.Count(id => !correctIds.Contains(id));
            int totalCorrect = correctIds.Count;

           
            decimal ratio = Math.Max(0, (decimal)(correctCounted - incorrectCounted) / totalCorrect);
            result.PointsEarned = Math.Round(question.Points * ratio, 2);
            result.Percentage = Math.Round(ratio * 100, 2);

            // Xác định đúng tuyệt đối
            result.IsCorrect = correctCounted == totalCorrect && incorrectCounted == 0;

          
            if (result.IsCorrect)
            {
                result.Feedback = "Correct! All answers selected.";
            }
            else if (correctCounted > 0)
            {
                result.Feedback = $"Partial credit: {correctCounted}/{totalCorrect} correct answers selected.";
            }
            else
            {
                result.Feedback = "Incorrect. No correct answers selected.";
            }

            return result;
        }
    }
}
