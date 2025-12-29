using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Services.FlashCard
{
    public interface ITeacherFlashCardCommandService
    {
        // Teacher tạo flashcard
        Task<ServiceResponse<FlashCardDto>> TeacherCreateFlashCard(CreateFlashCardDto createFlashCardDto, int teacherId);

        // Teacher tạo nhiều flashcard
        Task<ServiceResponse<List<FlashCardDto>>> TeacherBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto, int teacherId);

        // Teacher cập nhật flashcard
        Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int teacherId);

        // Teacher xóa flashcard
        Task<ServiceResponse<bool>> DeleteFlashCard(int flashCardId, int teacherId);
    }
}