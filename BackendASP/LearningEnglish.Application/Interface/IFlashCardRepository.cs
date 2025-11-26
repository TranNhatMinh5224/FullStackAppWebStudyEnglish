using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardRepository
    {
        // Basic CRUD operations
        Task<FlashCard?> GetByIdAsync(int flashCardId);
        Task<FlashCard?> GetByIdWithDetailsAsync(int flashCardId);
        Task<List<FlashCard>> GetByModuleIdAsync(int moduleId);
        Task<List<FlashCard>> GetByModuleIdWithDetailsAsync(int moduleId);
        Task<FlashCard> CreateAsync(FlashCard flashCard);
        Task<FlashCard> UpdateAsync(FlashCard flashCard);
        Task<bool> DeleteAsync(int flashCardId);
        Task<bool> ExistsAsync(int flashCardId);

        // Search and filter operations
        Task<List<FlashCard>> SearchFlashCardsAsync(string searchTerm, int? moduleId = null);
        Task<List<FlashCard>> GetFlashCardsByWordAsync(string word, int? moduleId = null);
        Task<bool> WordExistsInModuleAsync(string word, int moduleId, int? excludeFlashCardId = null); // chức năng kiểm tra từ đã tồn tại trong module chưa, dùng khi tạo mới hoặc cập nhật

        // Bulk operations
        Task<List<FlashCard>> CreateBulkAsync(List<FlashCard> flashCards);
        
        // Authorization helpers
        Task<FlashCard?> GetFlashCardWithModuleCourseAsync(int flashCardId);
    }
}
