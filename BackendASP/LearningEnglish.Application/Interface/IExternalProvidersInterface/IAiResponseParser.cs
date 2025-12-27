using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface;

/// <summary>
/// Interface for parsing AI responses (DIP - Dependency Inversion Principle)
/// </summary>
public interface IAiResponseParser
{
    AiGradingResult ParseGradingResponse(string content);
}
