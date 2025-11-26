using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardService
    {
        // Basic CRUD operations
        Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int? userId = null);
        Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int? userId = null);
        Task<ServiceResponse<FlashCardDto>> CreateFlashCardAsync(CreateFlashCardDto createFlashCardDto, int createdByUserId);
        Task<ServiceResponse<FlashCardDto>> UpdateFlashCardAsync(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int userId, string userRole);
        Task<ServiceResponse<bool>> DeleteFlashCardAsync(int flashCardId, int userId, string userRole);

        // Search and filter
        Task<ServiceResponse<List<ListFlashCardDto>>> SearchFlashCardsAsync(string searchTerm, int? moduleId = null, int? userId = null);
        
        // Bulk operations
        Task<ServiceResponse<List<FlashCardDto>>> CreateBulkFlashCardsAsync(BulkImportFlashCardDto bulkImportDto, int userId, string userRole);

        // Helper methods
        Task<bool> CheckTeacherFlashCardPermission(int flashCardId, int teacherId);
    }
}
