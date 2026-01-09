using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.FlashCard
{
    public interface IUserFlashCardService
    {
        // Lấy thông tin flashcard với progress của user
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int userId);
        
        // Lấy danh sách flashcard theo module với progress của user
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int userId);
    }
}
