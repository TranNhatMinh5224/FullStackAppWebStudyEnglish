using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptTeacherService
    {
        // Lấy danh sách bài làm của quiz (chỉ quiz trong course của teacher)
        Task<ServiceResponse<List<QuizAttemptDto>>> GetQuizAttemptsAsync(int quizId, int teacherId);
        
        // Lấy danh sách bài làm phân trang (chỉ quiz trong course của teacher)
        Task<ServiceResponse<PagedResult<QuizAttemptDto>>> GetQuizAttemptsPagedAsync(int quizId, int teacherId, PageRequest request);
        
        // Lấy thống kê quiz (chỉ quiz trong course của teacher)
        Task<ServiceResponse<object>> GetQuizAttemptStatsAsync(int quizId, int teacherId);
        
        // Lấy điểm số quiz (chỉ quiz trong course của teacher)
        Task<ServiceResponse<List<QuizScoreDto>>> GetQuizScoresAsync(int quizId, int teacherId);
        
        // Lấy điểm số quiz phân trang (chỉ quiz trong course của teacher)
        Task<ServiceResponse<PagedResult<QuizScoreDto>>> GetQuizScoresPagedAsync(int quizId, int teacherId, PageRequest request);
        
        // Lấy bài làm của người dùng (chỉ quiz trong course của teacher)
        Task<ServiceResponse<List<QuizAttemptDto>>> GetUserQuizAttemptsAsync(int userId, int quizId, int teacherId);
        
        // Lấy chi tiết bài làm với đáp án để teacher review (chỉ quiz trong course của teacher)
        Task<ServiceResponse<QuizAttemptDetailDto>> GetAttemptDetailForReviewAsync(int attemptId, int teacherId);
        
        // Force submit bài làm (dùng khi học sinh vi phạm nguyên tắc, quên nộp, hoặc gặp sự cố)
        Task<ServiceResponse<QuizAttemptResultDto>> ForceSubmitAttemptAsync(int attemptId, int teacherId);
    }
}

