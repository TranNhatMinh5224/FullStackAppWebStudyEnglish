using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizAttemptRepository
    {

        Task AddQuizAttemptAsync(QuizAttempt attempt); //  Thêm phương thức để thêm một QuizAttempt mới
        Task<QuizAttempt?> GetByIdAsync(int attemptId); // Phương thức để lấy QuizAttempt theo ID
        Task UpdateQuizAttemptAsync(QuizAttempt attempt); // Phương thức để cập nhật QuizAttempt
        Task DeleteQuizAttemptAsync(int attemptId); // Phương thức để xóa QuizAttempt
         // Lấy tất cả attempts của user cho một quiz
        Task<List<QuizAttempt>> GetByUserAndQuizAsync(int userId, int quizId);

        // Lấy attempts đang InProgress của user
        Task<QuizAttempt?> GetActiveAttemptAsync(int userId, int quizId);

        // Lấy tất cả attempts đang InProgress (cho auto-submit)
        Task<List<QuizAttempt>> GetInProgressAttemptsAsync();

        // Lấy tất cả attempts của một quiz (cho admin/teacher)
        Task<List<QuizAttempt>> GetByQuizIdAsync(int quizId);

        Task SaveChangesAsync(); // Phương thức để lưu các thay đổi vào cơ sở dữ liệu

    }
}
