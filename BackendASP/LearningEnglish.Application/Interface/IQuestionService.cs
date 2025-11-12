using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IQuestionService
    {
        Task<ServiceResponse<QuestionReadDto>> GetQuestionByIdAsync(int questionId);
        Task<ServiceResponse<List<QuestionReadDto>>> GetQuestionsByQuizGroupIdAsync(int quizGroupId);
        Task<ServiceResponse<List<QuestionReadDto>>> GetQuestionsByQuizSectionIdAsync(int quizSectionId);
        Task<ServiceResponse<QuestionReadDto>> AddQuestionAsync(QuestionCreateDto questionCreateDto);
        Task<ServiceResponse<QuestionReadDto>> UpdateQuestionAsync(int questionId, QuestionUpdateDto questionUpdateDto);
        Task<ServiceResponse<bool>> DeleteQuestionAsync(int questionId);
        Task<ServiceResponse<QuestionBulkResponseDto>> AddBulkQuestionsAsync(QuestionBulkCreateDto questionBulkCreateDto);
    }
}

