using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptRepository
    {
        // Thêm phương thức để thêm một QuizAttempt mới
        Task AddQuizAttemptAsync(QuizAttempt attempt);
        
        // Phương thức để lấy QuizAttempt theo ID
        Task<QuizAttempt?> GetByIdAsync(int attemptId);
        
        // Phương thức để cập nhật QuizAttempt
        Task UpdateQuizAttemptAsync(QuizAttempt attempt);
        
        // Phương thức để xóa QuizAttempt
        Task DeleteQuizAttemptAsync(int attemptId);
        
        // Lấy tất cả attempts của user cho một quiz
        Task<List<QuizAttempt>> GetByUserAndQuizAsync(int userId, int quizId);

        // Lấy attempts đang InProgress của user
        Task<QuizAttempt?> GetActiveAttemptAsync(int userId, int quizId);

        // Lấy tất cả attempts đang InProgress (cho auto-submit)
        Task<List<QuizAttempt>> GetInProgressAttemptsAsync();

        // Lấy tất cả attempts của một quiz (cho admin/teacher)
        Task<List<QuizAttempt>> GetByQuizIdAsync(int quizId);

        // Lấy danh sách attempts với phân trang
        Task<PagedResult<QuizAttempt>> GetQuizAttemptsPagedAsync(int quizId, PageRequest request);

        // Lấy điểm của học sinh với phân trang
        Task<PagedResult<QuizAttempt>> GetQuizScoresPagedAsync(int quizId, PageRequest request);

        // Phương thức để lưu các thay đổi vào cơ sở dữ liệu
        Task SaveChangesAsync();

    }
}
