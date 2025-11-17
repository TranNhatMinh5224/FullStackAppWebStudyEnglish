using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptService
    {
        // chuc nang bat dau lam bai thi quiz
        Task<ServiceResponse<QuizAttemptWithQuestionsDto>> StartQuizAttemptAsync(int quizId, int userId);

        // cap nhat diem cho cau hoi 
        Task<ServiceResponse<decimal>> UpdateScoreAsync(int quizId , UpdateScoreRequestDto request);

        // update cau tra loi va tinh diem ngay lap tuc
        Task<ServiceResponse<decimal>> UpdateAnswerAndScoreAsync(int attemptId, UpdateAnswerRequestDto request);

        // submit bai thi
        Task<ServiceResponse<QuizAttemptResultDto>> SubmitQuizAttemptAsync(int attemptId);

        // kiem tra va auto-submit neu het thoi gian
        Task<ServiceResponse<bool>> CheckAndAutoSubmitExpiredAttemptsAsync();

        // Chức năng resume quiz attempt
        Task<ServiceResponse<QuizAttemptWithQuestionsDto>> ResumeQuizAttemptAsync(int attemptId);


    }
}
