using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;


namespace LearningEnglish.Application.Interface
{
    public interface IQuizService
    {
        // Tạo quiz
        Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quiz, int? teacherId = null);
        
        // Lấy thông tin quiz
        Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId);
        
        // Lấy danh sách quiz theo assessment
        Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId);
        
        // Cập nhật quiz
        Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quiz, int? teacherId = null);
        
        // Xóa quiz
        Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId, int? teacherId = null);
    }
}