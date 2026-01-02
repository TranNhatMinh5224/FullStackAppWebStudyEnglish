using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Common.Helpers
{
    /// <summary>
    /// Helper class để tính toán điểm số cho Quiz
    /// </summary>
    public static class QuizScoreCalculator
    {
        /// <summary>
        /// Tính tổng điểm tối đa của Quiz dựa trên tất cả Questions
        /// </summary>
        /// <param name="quiz">Quiz entity với QuizSections, QuizGroups, Questions đã load</param>
        /// <returns>Tổng điểm tối đa</returns>
        public static decimal CalculateTotalPossibleScore(Quiz quiz)
        {
            if (quiz == null)
                throw new ArgumentNullException(nameof(quiz));

            decimal maxScore = 0;

            foreach (var section in quiz.QuizSections)
            {
                // Tính điểm từ questions trong groups
                foreach (var group in section.QuizGroups)
                {
                    maxScore += group.Questions.Sum(q => q.Points);
                }

                // Tính điểm từ standalone questions (không thuộc group)
                if (section.Questions != null)
                {
                    maxScore += section.Questions
                        .Where(q => q.QuizGroupId == null)
                        .Sum(q => q.Points);
                }
            }

            return maxScore;
        }
    }
}
