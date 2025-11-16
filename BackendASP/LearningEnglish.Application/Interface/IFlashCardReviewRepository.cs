using LearningEnglish.Domain.Entities;

namespace LearningEnglish.Application.Interface;

public interface IFlashCardReviewRepository
{
    Task<FlashCardReview?> GetByIdAsync(int reviewId);
    Task<FlashCardReview?> GetReviewAsync(int userId, int flashCardId);
    Task<List<FlashCardReview>> GetDueReviewsAsync(int userId, DateTime currentDate);
    Task<List<FlashCardReview>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20);
    Task<int> GetDueCountAsync(int userId, DateTime currentDate);
    Task<int> GetTotalReviewsCountAsync(int userId);
    Task<int> GetMasteredCardsCountAsync(int userId);
    Task<FlashCardReview> CreateAsync(FlashCardReview review);
    Task UpdateAsync(FlashCardReview review);
    Task DeleteAsync(int reviewId);
    Task<List<FlashCardReview>> GetRecentReviewsAsync(int userId, int days = 7);
}
