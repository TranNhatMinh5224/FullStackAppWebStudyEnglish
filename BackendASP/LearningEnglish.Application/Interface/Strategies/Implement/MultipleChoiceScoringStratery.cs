using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Strategies
{
    public class MultipleChoiceScoringStrategy : IScoringStrategy
    {
        public ScoringResultDto ScoreAnswer(Question question, QuizUserAnswer userAnswer)
        {
            // Khởi tạo kết quả mặc định
            var result = new ScoringResultDto
            {
                IsCorrect = false,
                PointsEarned = 0,
                Percentage = 0
            };

            //  Lấy đáp án đúng
            var correctOption = question.Options?.FirstOrDefault(o => o.IsCorrect);

            // Nếu câu hỏi không có đáp án đúng
            if (correctOption == null)
            {
                result.Feedback = "Question has no correct answer configured.";
                return result;
            }

            // Lưu ID đáp án đúng
            result.CorrectAnswerIds = new List<int> { correctOption.AnswerOptionId };

            // : Kiểm tra người dùng có trả lời không
            if (userAnswer.SelectedOptionId == null)
            {
                result.Feedback = "Not answered.";
                return result;
            }

            //  So sánh đáp án
            if (userAnswer.SelectedOptionId == correctOption.AnswerOptionId)
            {
                // Trả lời đúng
                result.IsCorrect = true;
                result.PointsEarned = question.Points;
                result.Percentage = 100;
                result.Feedback = "Correct!";
            }
            else
            {
                // Trả lời sai
                result.IsCorrect = false;
                result.PointsEarned = 0;
                result.Percentage = 0;

                // Tìm đáp án user đã chọn để hiển thị feedback
                var selectedOption = question.Options?.FirstOrDefault(o => o.AnswerOptionId == userAnswer.SelectedOptionId);

                if (selectedOption != null)
                {
                    result.Feedback = "Incorrect. You selected '{selectedOption.Text}'. The correct answer is '{correctOption.Text}'.";
                }
                else
                {
                    result.Feedback = "Incorrect. The correct answer is '{correctOption.Text}'.";
                }
            }

            return result;
        }
    }
}
