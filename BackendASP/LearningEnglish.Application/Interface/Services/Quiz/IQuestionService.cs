using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IQuestionService
    {
        // Lấy thông tin câu hỏi
        Task<ServiceResponse<QuestionReadDto>> GetQuestionByIdAsync(int questionId);
        
        // Lấy câu hỏi theo nhóm
        Task<ServiceResponse<List<QuestionReadDto>>> GetQuestionsByQuizGroupIdAsync(int quizGroupId);
        
        // Lấy câu hỏi theo section
        Task<ServiceResponse<List<QuestionReadDto>>> GetQuestionsByQuizSectionIdAsync(int quizSectionId);
        
        // Thêm câu hỏi
        Task<ServiceResponse<QuestionReadDto>> AddQuestionAsync(QuestionCreateDto questionCreateDto);
        
        // Cập nhật câu hỏi
        Task<ServiceResponse<QuestionReadDto>> UpdateQuestionAsync(int questionId, QuestionUpdateDto questionUpdateDto);
        
        // Xóa câu hỏi
        Task<ServiceResponse<bool>> DeleteQuestionAsync(int questionId);
        
        // Thêm nhiều câu hỏi cùng lúc
        Task<ServiceResponse<QuestionBulkResponseDto>> AddBulkQuestionsAsync(QuestionBulkCreateDto questionBulkCreateDto);
    }
}

