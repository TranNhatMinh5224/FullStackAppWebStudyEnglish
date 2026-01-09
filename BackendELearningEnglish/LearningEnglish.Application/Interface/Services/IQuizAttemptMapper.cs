using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface.Services;

/// <summary>
/// Interface cho QuizAttemptMapper - tuân thủ Dependency Inversion Principle
/// Application layer chỉ biết interface, không biết implementation details (bucket names, etc.)
/// </summary>
public interface IQuizAttemptMapper
{
    /// <summary>
    /// Shuffle quiz sections/groups/questions cho attempt
    /// </summary>
    List<AttemptQuizSectionDto> ShuffleQuizForAttempt(Quiz quiz, int attemptId);

    /// <summary>
    /// Map Question entity sang QuestionDto (cho group questions)
    /// </summary>
    QuestionDto MapToQuestionDto(Question question, int attemptId, bool shuffleAnswers);

    /// <summary>
    /// Map standalone Question sang QuizItemDto
    /// </summary>
    QuizItemDto MapToStandaloneQuestionItemDto(Question question, int attemptId, bool shuffleAnswers);

    /// <summary>
    /// Map AnswerOptions sang AnswerOptionDto và shuffle nếu cần
    /// </summary>
    List<AnswerOptionDto> MapToOptionDtos(Question question, int attemptId, bool shuffleAnswers);
}
