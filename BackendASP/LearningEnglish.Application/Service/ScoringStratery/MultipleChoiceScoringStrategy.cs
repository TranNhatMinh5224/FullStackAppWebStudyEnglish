using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using LearningEnglish.Application.Interface.Strategies;

namespace LearningEnglish.Application.Service.ScoringStrategies
{
    public class MultipleChoiceScoringStrategy : IScoringStrategy
    {
        public QuestionType Type => QuestionType.MultipleChoice;
        // cach code khac 
        // get {
        //     return QuestionType.MultipleChoice;
        // }
        public decimal CalculateScore(Question question, object userAnswer)
        {
            int selectedOptionId = (int)userAnswer; 
            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
            if (correctOption != null && correctOption.AnswerOptionId == selectedOptionId)
            {
                return question.Points; 
            }
            return 0m;
            
    } 
}
}
