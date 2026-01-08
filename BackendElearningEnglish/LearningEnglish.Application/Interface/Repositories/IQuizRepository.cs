using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizRepository
    {
        // Thêm quiz
        Task AddQuizAsync(Quiz quiz);
        
        // Lấy quiz theo ID
        Task<Quiz?> GetQuizByIdAsync(int quizId);
        
        // Lấy quiz theo assessment
        Task<List<Quiz>> GetQuizzesByAssessmentIdAsync(int assessmentId);
        
        // Cập nhật quiz
        Task UpdateQuizAsync(Quiz quiz);
        
        // Xóa quiz
        Task DeleteQuizAsync(int quizId);

        // Lấy quiz đầy đủ
        Task<Quiz?> GetFullQuizAsync(int quizId);
        
        // Kiểm tra có section
        Task<bool> HasSectionsAsync(int quizId);
        
        // Kiểm tra có group
        Task<bool> HasGroupsAsync(int quizId);
    }
}