using System;
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
            // Chuẩn hóa câu trả lời của học sinh
            string userAnswerStr = (userAnswer != null ? Convert.ToString(userAnswer) : string.Empty)
                                        .Trim()
                                        .ToLower();

            // Parse danh sách đáp án đúng từ CorrectAnswersJson
            var correctAnswersList = QuestionAnswerParser.ParseCorrectAnswers(question.CorrectAnswersJson);

            foreach (var correctAnswer in correctAnswersList)
            {
                if (userAnswerStr == (correctAnswer?.Trim() ?? string.Empty).ToLower())
                {
                    return question.Points;
                }
            }

            return 0m;
        }
    }
}
