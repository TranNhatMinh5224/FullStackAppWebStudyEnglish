using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptAdminService
    {
        // Lấy danh sách bài làm của quiz
        Task<ServiceResponse<List<QuizAttemptDto>>> GetQuizAttemptsAsync(int quizId);
        
        // Lấy danh sách bài làm phân trang
        Task<ServiceResponse<PagedResult<QuizAttemptDto>>> GetQuizAttemptsPagedAsync(int quizId, PageRequest request);
        
        // Lấy chi tiết bài làm với đáp án để admin review
        Task<ServiceResponse<QuizAttemptDetailDto>> GetAttemptDetailForReviewAsync(int attemptId);
        
        // Buộc nộp bài
        Task<ServiceResponse<QuizAttemptResultDto>> ForceSubmitAttemptAsync(int attemptId);
        
        // Lấy thống kê quiz
        Task<ServiceResponse<object>> GetQuizAttemptStatsAsync(int quizId);
        
        // Lấy điểm số quiz
        Task<ServiceResponse<List<QuizScoreDto>>> GetQuizScoresAsync(int quizId);
        
        // Lấy điểm số quiz phân trang
        Task<ServiceResponse<PagedResult<QuizScoreDto>>> GetQuizScoresPagedAsync(int quizId, PageRequest request);
        
        // Lấy bài làm của người dùng
        Task<ServiceResponse<List<QuizAttemptDto>>> GetUserQuizAttemptsAsync(int userId, int quizId);
    }
}
