using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface;


// Interface for parsing AI responses (DIP - Dependency Inversion Principle)

public interface IAiResponseParser
{
    AiGradingResult ParseGradingResponse(string content);
}
