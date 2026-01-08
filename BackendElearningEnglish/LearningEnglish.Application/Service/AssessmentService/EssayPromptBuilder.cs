using LearningEnglish.Application.Common.Prompts;
using LearningEnglish.Application.Interface.Strategies;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Service;


public class EssayPromptBuilder : IEssayPromptBuilder
{
    public string BuildGradingPrompt(Essay essay, string studentEssayContent, decimal maxScore)
    {
        // Use centralized prompt builder for consistency
        return EssayGradingPrompt.BuildPrompt(
            essay.Title,
            essay.Description ?? string.Empty,
            studentEssayContent,
            maxScore
        );
    }
}
