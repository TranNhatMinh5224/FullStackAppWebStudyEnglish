using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.FlashCard
{
    public interface ITeacherFlashCardService
    {
        // Teacher tạo flashcard
        Task<ServiceResponse<FlashCardDto>> TeacherCreateFlashCard(CreateFlashCardDto createFlashCardDto, int teacherId);

        // Teacher tạo nhiều flashcard
        Task<ServiceResponse<List<FlashCardDto>>> TeacherBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto, int teacherId);

        // Teacher cập nhật flashcard (RLS đã filter theo ownership)
        Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto updateFlashCardDto);

        // Teacher xóa flashcard (RLS đã filter theo ownership)
        Task<ServiceResponse<bool>> DeleteFlashCard(int flashCardId);

        // Teacher lấy flashcard theo ID (read-only)
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId);
        
        // Teacher lấy danh sách flashcard theo module (read-only)
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId);
    }
}
