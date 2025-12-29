using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.FlashCard
{
    public interface ITeacherFlashCardQueryService
    {
        // Teacher lấy flashcard theo ID
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int teacherId);

        // Teacher lấy danh sách flashcard theo module
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int teacherId);
    }
}