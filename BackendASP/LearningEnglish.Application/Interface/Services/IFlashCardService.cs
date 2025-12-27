using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardService
    {
        // Lấy thông tin flashcard
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int? userId = null);
        
        // Lấy danh sách flashcard theo module
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int? userId = null);
        
        // Admin tạo flashcard
        Task<ServiceResponse<FlashCardDto>> AdminCreateFlashCard(CreateFlashCardDto createFlashCardDto);

        // Teacher tạo flashcard
        Task<ServiceResponse<FlashCardDto>> TeacherCreateFlashCard(CreateFlashCardDto createFlashCardDto, int teacherId);

        // Cập nhật flashcard (RLS đã filter theo ownership)
        Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto updateFlashCardDto);

        // Xóa flashcard (RLS đã filter theo ownership)
        Task<ServiceResponse<bool>> DeleteFlashCard(int flashCardId);

        // Admin tạo nhiều flashcard
        Task<ServiceResponse<List<FlashCardDto>>> AdminBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto);

        // Teacher tạo nhiều flashcard
        Task<ServiceResponse<List<FlashCardDto>>> TeacherBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto, int teacherId);
    }
}
