using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IFlashCardReviewRepository
{
    // Lấy review theo ID
    // RLS: User chỉ xem reviews của chính mình, Admin xem tất cả (có permission)
    Task<FlashCardReview?> GetByIdAsync(int reviewId);
    
    // Lấy review của user cho flashcard
    // RLS: User chỉ xem reviews của chính mình, Admin xem tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<FlashCardReview?> GetReviewAsync(int userId, int flashCardId);
    
    // Lấy review đến hạn ôn tập
    // RLS: User chỉ xem reviews của chính mình, Admin xem tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<List<FlashCardReview>> GetDueReviewsAsync(int userId, DateTime currentDate);
    
    // Lấy review của user với phân trang
    // RLS: User chỉ xem reviews của chính mình, Admin xem tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<List<FlashCardReview>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20);
    
    // Đếm số review đến hạn
    // RLS: User chỉ đếm reviews của chính mình, Admin đếm tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<int> GetDueCountAsync(int userId, DateTime currentDate);
    
    // Đếm tổng số review
    // RLS: User chỉ đếm reviews của chính mình, Admin đếm tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<int> GetTotalReviewsCountAsync(int userId);
    
    // Đếm số thẻ đã thuộc
    // RLS: User chỉ đếm reviews của chính mình, Admin đếm tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<int> GetMasteredCardsCountAsync(int userId);
    
    // Tạo review
    Task<FlashCardReview> CreateAsync(FlashCardReview review);
    
    // Cập nhật review
    Task UpdateAsync(FlashCardReview review);
    
    // Xóa review
    Task DeleteAsync(int reviewId);
    
    // Lấy review gần đây
    // RLS: User chỉ xem reviews của chính mình, Admin xem tất cả (có permission)
    // userId parameter: Defense in depth (RLS + userId filter)
    Task<List<FlashCardReview>> GetRecentReviewsAsync(int userId, int days = 7);
}
