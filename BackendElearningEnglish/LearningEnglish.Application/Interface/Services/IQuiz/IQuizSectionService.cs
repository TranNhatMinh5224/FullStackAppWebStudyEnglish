using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizSectionService
    {
        // Tạo section cho quiz
        Task<ServiceResponse<QuizSectionDto>> CreateQuizSectionAsync(CreateQuizSectionDto createDto);
        
        // Lấy thông tin section
        Task<ServiceResponse<QuizSectionDto>> GetQuizSectionByIdAsync(int quizSectionId);
        
        // Lấy danh sách section của quiz
        Task<ServiceResponse<List<QuizSectionDto>>> GetQuizSectionsByQuizIdAsync(int quizId);
        
        // Cập nhật section
        Task<ServiceResponse<QuizSectionDto>> UpdateQuizSectionAsync(int quizSectionId, UpdateQuizSectionDto updateDto);
        
        // Xóa section
        Task<ServiceResponse<bool>> DeleteQuizSectionAsync(int quizSectionId);
    }
}
