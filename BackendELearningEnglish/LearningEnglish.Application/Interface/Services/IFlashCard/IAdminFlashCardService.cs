using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.FlashCard
{
    public interface IAdminFlashCardService
    {
        // Admin tạo flashcard
        Task<ServiceResponse<FlashCardDto>> AdminCreateFlashCard(CreateFlashCardDto createFlashCardDto);

        // Admin tạo nhiều flashcard
        Task<ServiceResponse<List<FlashCardDto>>> AdminBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto);

        // Admin cập nhật flashcard
        Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto updateFlashCardDto);

        // Admin xóa flashcard
        Task<ServiceResponse<bool>> DeleteFlashCard(int flashCardId);

        // Admin lấy flashcard theo ID (read-only)
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId);
        
        // Admin lấy danh sách flashcard theo module (read-only)
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId);
    }
}
