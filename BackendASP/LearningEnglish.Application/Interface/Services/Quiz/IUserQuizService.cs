using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IUserQuizService
    {
        // Lấy thông tin quiz (User chỉ xem)
        Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId, int userId);
        
        // Lấy danh sách quiz theo assessment (User chỉ xem)
        Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId, int userId);
    }
}
