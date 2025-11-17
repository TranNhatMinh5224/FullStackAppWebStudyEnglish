using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptService
    {
        // chuc nang bat dau lam bai thi quiz
        Task<ServiceResponse<QuizAttemptWithQuestionsDto>> StartQuizAttemptAsync(int quizId, int userId);

        // update cau tra loi va tinh diem ngay lap tuc (real-time scoring)
        // Khi user làm câu nào, sẽ update answer và chấm điểm luôn
        // Nếu làm đúng rồi sửa lại sai thì điểm sẽ từ có điểm thành 0 điểm
        Task<ServiceResponse<decimal>> UpdateAnswerAndScoreAsync(int attemptId, UpdateAnswerRequestDto request);

        // submit bai thi
        Task<ServiceResponse<QuizAttemptResultDto>> SubmitQuizAttemptAsync(int attemptId);

        // kiem tra va auto-submit neu het thoi gian
        Task<ServiceResponse<bool>> CheckAndAutoSubmitExpiredAttemptsAsync();

        // Chức năng resume quiz attempt
        Task<ServiceResponse<QuizAttemptWithQuestionsDto>> ResumeQuizAttemptAsync(int attemptId);


    }
}
