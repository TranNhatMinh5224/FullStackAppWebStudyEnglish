using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptService
    {
        // Tạo attempt mới - trả về bộ đề
        Task<ServiceResponse<StartQuizAttemptResponseDto>> StartAttemptAsync(int userId, StartQuizAttemptRequestDto request);
        
        // Update câu trả lời realtime
        // Task<ServiceResponse<bool>> UpdateAnswerAsync(int userId, int attemptId, UpdateAnswerDto answerDto);
        
        // Lấy trạng thái attempt
        Task<ServiceResponse<QuizAttemptDto>> GetAttemptAsync(int userId, int attemptId);
        
        // Nộp bài và chuyển status sang Submitted
        Task<ServiceResponse<QuizAttemptDto>> FinishAttemptAsync(int userId, int attemptId);
    }
}
