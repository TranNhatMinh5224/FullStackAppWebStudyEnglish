using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface.Strategies;

public interface IEssayPromptBuilder
{
    string BuildGradingPrompt(Essay essay, string studentEssayContent, decimal maxScore);
}
