using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptService
    {
        // Bắt đầu làm bài quiz
        Task<ServiceResponse<QuizAttemptWithQuestionsDto>> StartQuizAttemptAsync(int quizId, int userId);

        // Cập nhật câu trả lời và chấm điểm
        Task<ServiceResponse<decimal>> UpdateAnswerAndScoreAsync(int attemptId, UpdateAnswerRequestDto request);

        // Nộp bài quiz
        Task<ServiceResponse<QuizAttemptResultDto>> SubmitQuizAttemptAsync(int attemptId);

        // Tự động nộp bài hết giờ
        Task<ServiceResponse<bool>> CheckAndAutoSubmitExpiredAttemptsAsync();

        // Tiếp tục làm bài
        Task<ServiceResponse<QuizAttemptWithQuestionsDto>> ResumeQuizAttemptAsync(int attemptId);
    }
}
