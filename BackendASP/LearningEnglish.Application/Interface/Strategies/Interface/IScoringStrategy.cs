using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IScoringStrategy
    {
        ScoringResultDto ScoreAnswer(Question question, QuizUserAnswer userAnswer);
    }
}