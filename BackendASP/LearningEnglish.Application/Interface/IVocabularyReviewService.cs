using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface;

public interface IVocabularyReviewService
{
    // Lấy danh sách từ cần ôn tập hôm nay
    Task<ServiceResponse<List<VocabularyReviewDto>>> GetDueReviewsAsync(int userId);

    // Lấy từ mới để học
    Task<ServiceResponse<List<FlashCardDto>>> GetNewCardsAsync(int userId, int limit = 10);

    // Bắt đầu ôn tập một từ
    Task<ServiceResponse<VocabularyReviewDto>> StartReviewAsync(int userId, int flashCardId);

    // Submit kết quả ôn tập (quality score 0-5)
    Task<ServiceResponse<VocabularyReviewResultDto>> SubmitReviewAsync(int reviewId, int quality);

    // Lấy thống kê vocabulary review
    Task<ServiceResponse<VocabularyStatsDto>> GetVocabularyStatsAsync(int userId);

    // Lấy lịch sử ôn tập gần đây
    Task<ServiceResponse<List<VocabularyReviewDto>>> GetRecentReviewsAsync(int userId, int days = 7);

    // Reset progress cho một từ (quên rồi)
    Task<ServiceResponse<bool>> ResetCardProgressAsync(int userId, int flashCardId);
}
