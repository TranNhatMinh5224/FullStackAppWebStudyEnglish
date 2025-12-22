using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface
{
    public interface IFlashCardRepository
    {
        // Lấy flashcard theo ID
        Task<FlashCard?> GetByIdAsync(int flashCardId);
        
        // Lấy flashcard với chi tiết
        Task<FlashCard?> GetByIdWithDetailsAsync(int flashCardId);
        
        // Lấy flashcard theo module
        Task<List<FlashCard>> GetByModuleIdAsync(int moduleId);
        
        // Lấy flashcard theo module với chi tiết
        Task<List<FlashCard>> GetByModuleIdWithDetailsAsync(int moduleId);
        
        // Đếm số flashcard trong module
        Task<int> GetFlashCardCountByModuleAsync(int moduleId);
        
        // Tạo flashcard
        Task<FlashCard> CreateAsync(FlashCard flashCard);
        
        // Cập nhật flashcard
        Task<FlashCard> UpdateAsync(FlashCard flashCard);
        
        // Xóa flashcard
        Task<bool> DeleteAsync(int flashCardId);
        
        // Kiểm tra flashcard tồn tại
        Task<bool> ExistsAsync(int flashCardId);

        // Tạo nhiều flashcard
        Task<List<FlashCard>> CreateBulkAsync(List<FlashCard> flashCards);

        // Lấy flashcard với module và course
        Task<FlashCard?> GetFlashCardWithModuleCourseAsync(int flashCardId);
    }
}
