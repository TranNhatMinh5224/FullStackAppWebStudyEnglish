using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizSectionRepository
    {
        // Lấy quiz section theo ID
        Task<QuizSection?> GetQuizSectionByIdAsync(int quizSectionId);
        
        // Lấy quiz section theo quiz
        Task<List<QuizSection>> GetQuizSectionsByQuizIdAsync(int quizId);
        
        // Tạo quiz section
        Task<QuizSection> CreateQuizSectionAsync(QuizSection quizSection);
        
        // Cập nhật quiz section
        Task<QuizSection> UpdateQuizSectionAsync(QuizSection quizSection);
        
        // Xóa quiz section
        Task<bool> DeleteQuizSectionAsync(int quizSectionId);
        
        // Kiểm tra quiz section tồn tại
        Task<bool> QuizSectionExistsAsync(int quizSectionId);
        
        // Lấy quiz theo ID
        Task<Quiz?> GetQuizByIdAsync(int quizId);

        // Save changes
        Task SaveChangesAsync();

        // Add quiz section without auto save
        Task AddQuizSectionAsync(QuizSection quizSection);
    }
}
