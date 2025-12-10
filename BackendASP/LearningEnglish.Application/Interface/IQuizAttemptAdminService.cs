using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptAdminService
    {
        Task<ServiceResponse<List<QuizAttempt>>> GetQuizAttemptsAsync(int quizId);
        Task<ServiceResponse<PagedResult<QuizAttempt>>> GetQuizAttemptsPagedAsync(int quizId, PageRequest request);
        Task<ServiceResponse<QuizAttempt>> GetAttemptDetailsAsync(int attemptId);
        Task<ServiceResponse<QuizAttemptResultDto>> ForceSubmitAttemptAsync(int attemptId);
        Task<ServiceResponse<object>> GetQuizAttemptStatsAsync(int quizId);
        Task<ServiceResponse<List<object>>> GetQuizScoresAsync(int quizId);
        Task<ServiceResponse<PagedResult<object>>> GetQuizScoresPagedAsync(int quizId, PageRequest request);
        Task<ServiceResponse<List<QuizAttempt>>> GetUserQuizAttemptsAsync(int userId, int quizId);
    }
}
