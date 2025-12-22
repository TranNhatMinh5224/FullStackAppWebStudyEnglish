using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardService
    {
        // Lấy thông tin flashcard
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int? userId = null);
        
        // Lấy danh sách flashcard theo module
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int? userId = null);
        
        // Lấy flashcard phân trang
        Task<ServiceResponse<PagedResult<ListFlashCardDto>>> GetFlashCardsByModuleIdPaginatedAsync(int moduleId, PageRequest request, int? userId = null);
        
        // Tạo flashcard
        Task<ServiceResponse<FlashCardDto>> CreateFlashCardAsync(CreateFlashCardDto createFlashCardDto, int createdByUserId);
        
        // Cập nhật flashcard
        Task<ServiceResponse<FlashCardDto>> UpdateFlashCardAsync(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int userId, string userRole);
        
        // Xóa flashcard
        Task<ServiceResponse<bool>> DeleteFlashCardAsync(int flashCardId, int userId, string userRole);

        // Tạo nhiều flashcard
        Task<ServiceResponse<List<FlashCardDto>>> CreateBulkFlashCardsAsync(BulkImportFlashCardDto bulkImportDto, int userId, string userRole);

        // Kiểm tra quyền teacher
        Task<bool> CheckTeacherFlashCardPermission(int flashCardId, int teacherId);
    }
}
