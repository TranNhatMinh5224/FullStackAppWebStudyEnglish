using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizGroupRepository
    {
        // Lấy quiz group theo ID
        Task<QuizGroup?> GetQuizGroupByIdAsync(int quizGroupId);
        
        // Lấy quiz group theo quiz section
        Task<List<QuizGroup>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId);
        
        // Tạo quiz group
        Task<QuizGroup> CreateQuizGroupAsync(QuizGroup quizGroup);
        
        // Cập nhật quiz group
        Task<QuizGroup> UpdateQuizGroupAsync(QuizGroup quizGroup);
        
        // Xóa quiz group
        Task<bool> DeleteQuizGroupAsync(int quizGroupId);
        
        // Kiểm tra quiz group tồn tại
        Task<bool> QuizGroupExistsAsync(int quizGroupId);
        
        // Lấy quiz section theo ID
        Task<QuizSection?> GetQuizSectionByIdAsync(int quizSectionId);

        // Save changes
        Task SaveChangesAsync();

        // Add quiz group without auto save
        Task AddQuizGroupAsync(QuizGroup quizGroup);
    }
}
