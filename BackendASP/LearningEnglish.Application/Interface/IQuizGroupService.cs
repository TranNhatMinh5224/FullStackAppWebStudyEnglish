using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IQuizGroupService
    {
        Task<ServiceResponse<QuizGroupDto>> CreateQuizGroupAsync(CreateQuizGroupDto createDto);
        Task<ServiceResponse<QuizGroupDto>> GetQuizGroupByIdAsync(int quizGroupId);
        Task<ServiceResponse<List<QuizGroupDto>>> GetQuizGroupsByQuizSectionIdAsync(int quizSectionId);
        Task<ServiceResponse<QuizGroupDto>> UpdateQuizGroupAsync(int quizGroupId, UpdateQuizGroupDto updateDto);
        Task<ServiceResponse<bool>> DeleteQuizGroupAsync(int quizGroupId);
    }
}
