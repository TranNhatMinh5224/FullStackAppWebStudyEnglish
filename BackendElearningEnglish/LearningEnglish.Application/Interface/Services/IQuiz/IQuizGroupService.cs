using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizGroupService
    {
        // Tạo nhóm câu hỏi
        Task<ServiceResponse<QuizGroupDto>> CreateQuizGroupAsync(CreateQuizGroupDto createDto);
        
        // Lấy thông tin nhóm
        Task<ServiceResponse<QuizGroupDto>> GetQuizGroupByIdAsync(int quizGroupId);
        
        // Lấy danh sách nhóm theo section
        Task<ServiceResponse<List<QuizGroupDto>>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId);
        
        // Cập nhật nhóm
        Task<ServiceResponse<QuizGroupDto>> UpdateQuizGroupAsync(int quizGroupId, UpdateQuizGroupDto updateDto);
        
        // Xóa nhóm
        Task<ServiceResponse<bool>> DeleteQuizGroupAsync(int quizGroupId);
    }
}
