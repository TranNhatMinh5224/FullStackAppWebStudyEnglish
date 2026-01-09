using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAdminQuizService
    {
        // Tạo quiz (Admin - không cần teacherId)
        Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quiz);
        
        // Lấy thông tin quiz
        Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId);
        
        // Lấy danh sách quiz theo assessment
        Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId);
        
        // Cập nhật quiz
        Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quiz);
        
        // Xóa quiz (không check ownership)
        Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId);
    }
}
