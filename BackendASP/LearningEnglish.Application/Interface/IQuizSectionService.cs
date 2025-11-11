using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizSectionService
    {
        Task<ServiceResponse<QuizSectionDto>> CreateQuizSectionAsync(CreateQuizSectionDto createDto);
        Task<ServiceResponse<QuizSectionDto>> GetQuizSectionByIdAsync(int quizSectionId);
        Task<ServiceResponse<List<QuizSectionDto>>> GetQuizSectionsByQuizIdAsync(int quizId);
        Task<ServiceResponse<QuizSectionDto>> UpdateQuizSectionAsync(int quizSectionId, UpdateQuizSectionDto updateDto);
        Task<ServiceResponse<bool>> DeleteQuizSectionAsync(int quizSectionId);
    }
}
