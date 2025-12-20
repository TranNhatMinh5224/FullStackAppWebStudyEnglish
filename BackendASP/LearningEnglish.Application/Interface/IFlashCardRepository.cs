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
        Task<int> GetFlashCardCountByModuleAsync(int moduleId);
        Task<FlashCard> CreateAsync(FlashCard flashCard);
        Task<FlashCard> UpdateAsync(FlashCard flashCard);
        Task<bool> DeleteAsync(int flashCardId);
        Task<bool> ExistsAsync(int flashCardId);

        // Bulk operations
        Task<List<FlashCard>> CreateBulkAsync(List<FlashCard> flashCards);

        // Authorization helpers
        Task<FlashCard?> GetFlashCardWithModuleCourseAsync(int flashCardId);
    }
}
