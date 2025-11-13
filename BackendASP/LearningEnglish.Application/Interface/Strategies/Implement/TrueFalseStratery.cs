using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Strategies
{
    public class TrueFalseScoringStrategy : IScoringStrategy
    {
        public ScoringResultDto ScoreAnswer(Question question, QuizUserAnswer userAnswer)
        {
            // TRUE/FALSE
            var mcqStrategy = new MultipleChoiceScoringStrategy();
            var result = mcqStrategy.ScoreAnswer(question, userAnswer);

            // Tùy chỉnh feedback cho True/False
            if (result.IsCorrect)
            {
                result.Feedback = "Correct!";
            }
            else if (userAnswer.SelectedOptionId == null)
            {
                result.Feedback = "Not answered.";
            }
            else
            {
                // Tìm đáp án đúng và đáp án user chọn
                var correctOption = question.Options?.FirstOrDefault(o => o.IsCorrect);
                var selectedOption = question.Options?.FirstOrDefault(o => o.AnswerOptionId == userAnswer.SelectedOptionId);

                if (correctOption != null)
                {
                    result.Feedback = selectedOption != null
                        ? $"Incorrect. You selected '{selectedOption.Text}'. The correct answer is '{correctOption.Text}'."
                        : $"Incorrect. The correct answer is '{correctOption.Text}'.";
                }
                else
                {
                    result.Feedback = "Incorrect.";
                }
            }

            return result;
        }
    }
}
