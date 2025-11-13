using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Strategies;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Service
{
    public class QuizScoringService
    {
        // Dictionary lưu các strategy theo loại câu hỏi
        private readonly Dictionary<TypeQuestion, IScoringStrategy> _strategies;

        // Constructor: đăng ký các strategy
        public QuizScoringService()
        {
            _strategies = new Dictionary<TypeQuestion, IScoringStrategy>
            {
                { TypeQuestion.MultipleChoice, new MultipleChoiceScoringStrategy() },
                { TypeQuestion.MultipleAnswers, new MultipleAnswersScoringStrategy() },
                { TypeQuestion.TrueFalse, new TrueFalseScoringStrategy() },
                // { TypeQuestion.Essay, new EssayScoringStrategy() }
            };
        }

        // Chấm điểm 1 câu trả lời
        public ScoringResultDto ScoreAnswer(Question question, QuizUserAnswer userAnswer)
        {
            // Tìm strategy theo loại câu hỏi
            if (!_strategies.TryGetValue(question.Type, out var strategy))
            {
                
                return new ScoringResultDto
                {
                    IsCorrect = false,
                    PointsEarned = 0,
                    Percentage = 0,
                    Feedback = $"Loại câu hỏi '{question.Type}' chưa hỗ trợ chấm tự động."
                };
            }

            // Gọi strategy để chấm điểm
            var result = strategy.ScoreAnswer(question, userAnswer);

            
            if (!string.IsNullOrWhiteSpace(result.Feedback))
            {
                if (result.Feedback == "Correct!") result.Feedback = "Trả lời đúng!";
                else if (result.Feedback == "Not answered.") result.Feedback = "Chưa trả lời.";
                else if (result.Feedback.StartsWith("Incorrect"))
                {
                    result.Feedback = result.Feedback
                        .Replace("Incorrect.", "Trả lời sai.")
                        .Replace("You selected", "Bạn chọn")
                        .Replace("The correct answer is", "Đáp án đúng là");
                }
                else if (result.Feedback.StartsWith("Answer submitted"))
                {
                    result.Feedback = result.Feedback.Replace("Answer submitted.", "Đã gửi bài.").Replace("Waiting for teacher review.", "Đang chờ giáo viên chấm.");
                }
            }

            return result;
        }

        // Chấm điểm toàn bộ attempt
        public QuizAttemptScoringResultDto ScoreAttempt(
            QuizAttempt attempt,
            List<Question> questions,
            decimal? passingScore = null)
        {
            var questionResults = new List<QuestionScoringDto>();
            decimal totalPointsEarned = 0;
            decimal totalMaxPoints = 0;

            // Duyệt từng câu hỏi
            foreach (var question in questions)
            {
                totalMaxPoints += question.Points;

                // Lấy câu trả lời của user
                var userAnswer = attempt.Answers?.FirstOrDefault(a => a.QuestionId == question.QuestionId);

                if (userAnswer == null)
                {
                    // User chưa trả lời → feedback tiếng Việt
                    questionResults.Add(new QuestionScoringDto
                    {
                        QuestionId = question.QuestionId,
                        IsCorrect = false,
                        PointsEarned = 0,
                        MaxPoints = question.Points,
                        Percentage = 0,
                        Feedback = "Chưa trả lời."
                    });
                    continue;
                }

                // Chấm điểm câu này
                var result = ScoreAnswer(question, userAnswer);
                totalPointsEarned += result.PointsEarned;

                // Lưu kết quả từng câu
                questionResults.Add(new QuestionScoringDto
                {
                    QuestionId = question.QuestionId,
                    IsCorrect = result.IsCorrect,
                    PointsEarned = result.PointsEarned,
                    MaxPoints = question.Points,
                    Percentage = result.Percentage,
                    Feedback = result.Feedback,
                    CorrectAnswerIds = result.CorrectAnswerIds
                });
            }

            // Tính tỷ lệ %
            decimal percentage = totalMaxPoints > 0
                ? Math.Round((totalPointsEarned / totalMaxPoints) * 100, 2)
                : 0;

            // Trả về kết quả tổng hợp
            return new QuizAttemptScoringResultDto
            {
                AttemptId = attempt.AttemptId,
                TotalPointsEarned = totalPointsEarned,
                TotalMaxPoints = totalMaxPoints,
                Percentage = percentage,
                // IsPassed = passingScore.HasValue && percentage >= passingScore.Value,
                QuestionResults = questionResults
            };
        }
    }
}
