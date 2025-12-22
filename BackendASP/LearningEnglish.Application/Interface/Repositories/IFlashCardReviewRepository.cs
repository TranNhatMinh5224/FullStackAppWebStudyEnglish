using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IFlashCardReviewRepository
{
    // Lấy review theo ID
    Task<FlashCardReview?> GetByIdAsync(int reviewId);
    
    // Lấy review của user cho flashcard
    Task<FlashCardReview?> GetReviewAsync(int userId, int flashCardId);
    
    // Lấy review đến hạn ôn tập
    Task<List<FlashCardReview>> GetDueReviewsAsync(int userId, DateTime currentDate);
    
    // Lấy review của user với phân trang
    Task<List<FlashCardReview>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20);
    
    // Đếm số review đến hạn
    Task<int> GetDueCountAsync(int userId, DateTime currentDate);
    
    // Đếm tổng số review
    Task<int> GetTotalReviewsCountAsync(int userId);
    
    // Đếm số thẻ đã thuộc
    Task<int> GetMasteredCardsCountAsync(int userId);
    
    // Tạo review
    Task<FlashCardReview> CreateAsync(FlashCardReview review);
    
    // Cập nhật review
    Task UpdateAsync(FlashCardReview review);
    
    // Xóa review
    Task DeleteAsync(int reviewId);
    
    // Lấy review gần đây
    Task<List<FlashCardReview>> GetRecentReviewsAsync(int userId, int days = 7);
}
