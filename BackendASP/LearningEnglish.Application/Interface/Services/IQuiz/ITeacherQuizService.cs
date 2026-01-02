using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface ITeacherQuizService
    {
        // Tạo quiz (Teacher - validate assessment ownership)
        Task<ServiceResponse<QuizDto>> CreateQuizAsync(QuizCreateDto quiz, int teacherId);
        
        // Lấy thông tin quiz (validate ownership)
        Task<ServiceResponse<QuizDto>> GetQuizByIdAsync(int quizId, int teacherId);
        
        // Lấy danh sách quiz theo assessment (validate ownership)
        Task<ServiceResponse<List<QuizDto>>> GetQuizzesByAssessmentIdAsync(int assessmentId, int teacherId);
        
        // Cập nhật quiz (validate ownership)
        Task<ServiceResponse<QuizDto>> UpdateQuizAsync(int quizId, QuizUpdateDto quiz, int teacherId);
        
        // Xóa quiz (validate ownership)
        Task<ServiceResponse<bool>> DeleteQuizAsync(int quizId, int teacherId);
    }
}
