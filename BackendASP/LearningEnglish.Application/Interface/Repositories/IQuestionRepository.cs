using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IQuestionRepository
    {
        // Thêm câu hỏi
        Task AddQuestionAsync(Question question);
        
        // Thêm nhiều câu hỏi
        Task AddBulkQuestionsAsync(List<Question> questions);
        
        // Thêm nhiều câu hỏi với transaction
        Task<List<int>> AddBulkQuestionsWithTransactionAsync(List<Question> questions);
        
        // Lấy câu hỏi theo ID
        Task<Question?> GetQuestionByIdAsync(int questionId);
        
        // Lấy câu hỏi theo quiz group
        Task<List<Question>> GetQuestionsByQuizGroupIdAsync(int quizGroupId);
        
        // Lấy câu hỏi theo quiz section
        Task<List<Question>> GetQuestionsByQuizSectionIdAsync(int quizSectionId);
        
        // Cập nhật câu hỏi
        Task UpdateQuestionAsync(Question question);
        
        // Xóa câu hỏi
        Task DeleteQuestionAsync(int questionId);
    }
}
