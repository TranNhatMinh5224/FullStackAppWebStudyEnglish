using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptRepository
    {
        // Thêm bài làm quiz
        Task AddQuizAttemptAsync(QuizAttempt attempt);
        
        // Lấy bài làm theo ID
        Task<QuizAttempt?> GetByIdAsync(int attemptId);
        
        // Cập nhật bài làm quiz
        Task UpdateQuizAttemptAsync(QuizAttempt attempt);
        
        // Xóa bài làm quiz
        Task DeleteQuizAttemptAsync(int attemptId);
        
        // Lấy tất cả bài làm của user cho quiz
        Task<List<QuizAttempt>> GetByUserAndQuizAsync(int userId, int quizId);

        // Lấy bài làm đang thực hiện
        Task<QuizAttempt?> GetActiveAttemptAsync(int userId, int quizId);

        // Lấy tất cả bài làm đang thực hiện
        Task<List<QuizAttempt>> GetInProgressAttemptsAsync();

        // Lấy tất cả bài làm của quiz
        Task<List<QuizAttempt>> GetByQuizIdAsync(int quizId);

        // Lấy bài làm với phân trang
        Task<PagedResult<QuizAttempt>> GetQuizAttemptsPagedAsync(int quizId, PageRequest request);

        // Lấy điểm với phân trang
        Task<PagedResult<QuizAttempt>> GetQuizScoresPagedAsync(int quizId, PageRequest request);

        // Lưu thay đổi
        Task SaveChangesAsync();

    }
}
