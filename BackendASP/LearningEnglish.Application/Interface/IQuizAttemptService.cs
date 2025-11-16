using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptService
    {
        Task<ServiceResponse<QuizAttemptDto>> StartQuizAttemptAsync(int quizId, int userId);
        Task<ServiceResponse<decimal>> UpdateScoreAsync(int attemptId, decimal newScore);

        // chuc nang nop bai thi quiz
        Task<ServiceResponse<QuizAttemptResultDto>> SubmitQuizAttemptAsync(int attemptId);
    }
}
