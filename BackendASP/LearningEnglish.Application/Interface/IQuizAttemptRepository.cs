using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptRepository
    {
        Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId);
        Task<QuizAttempt> CreateAttemptAsync(QuizAttempt attempt);
        Task UpdateAttemptAsync(QuizAttempt attempt);
        Task<bool> UserHasActiveAttemptAsync(int quizId, int userId);
        Task<QuizAttempt?> GetLatestAttemptAsync(int quizId, int userId); // Lấy attempt mới nhất để check AttemptNumber
    }
}