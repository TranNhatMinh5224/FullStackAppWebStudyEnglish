using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;


namespace LearningEnglish.Application.Interface
{
    public interface IQuizService
    {
        Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quiz);
        Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId);
        Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId);
        Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quiz);
        Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId);
    }
}