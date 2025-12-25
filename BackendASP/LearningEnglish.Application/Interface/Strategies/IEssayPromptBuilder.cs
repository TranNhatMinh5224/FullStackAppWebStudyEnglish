using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface.Strategies;

/// <summary>
/// Strategy interface for building essay grading prompts (SRP, OCP)
/// </summary>
public interface IEssayPromptBuilder
{
    string BuildGradingPrompt(Essay essay, string studentEssayContent, decimal maxScore);
}
