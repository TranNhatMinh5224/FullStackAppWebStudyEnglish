using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service;

public class VocabularyReviewService : IVocabularyReviewService
{
    private readonly IFlashCardReviewRepository _reviewRepo;
    private readonly IFlashCardService _flashCardService;
    private readonly IStreakService _streakService;
    private readonly ILogger<VocabularyReviewService> _logger;

    public VocabularyReviewService(
        IFlashCardReviewRepository reviewRepo,
        IFlashCardService flashCardService,
        IStreakService streakService,
        ILogger<VocabularyReviewService> logger)
    {
        _reviewRepo = reviewRepo;
        _flashCardService = flashCardService;
        _streakService = streakService;
        _logger = logger;
    }

    public async Task<ServiceResponse<List<VocabularyReviewDto>>> GetDueReviewsAsync(int userId)
    {
        try
        {
            var dueReviews = await _reviewRepo.GetDueReviewsAsync(userId, DateTime.UtcNow);

            var reviewDtos = dueReviews.Select(r => new VocabularyReviewDto
            {
                ReviewId = r.FlashCardReviewId,
                FlashCardId = r.FlashCardId,
                FlashCard = MapToFlashCardDto(r.FlashCard),
                Quality = r.Quality,
                EasinessFactor = r.EasinessFactor,
                IntervalDays = r.IntervalDays,
                RepetitionCount = r.RepetitionCount,
                NextReviewDate = r.NextReviewDate,
                ReviewedAt = r.ReviewedAt,
                ReviewStatus = GetReviewStatus(r)
            }).ToList();

            return new ServiceResponse<List<VocabularyReviewDto>>
            {
                Success = true,
                Data = reviewDtos,
                Message = $"Tìm thấy {reviewDtos.Count} từ cần ôn tập"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting due reviews for user {UserId}", userId);
            return new ServiceResponse<List<VocabularyReviewDto>>
            {
                Success = false,
                Message = "Không thể tải danh sách ôn tập"
            };
        }
    }

    public async Task<ServiceResponse<List<FlashCardDto>>> GetNewCardsAsync(int userId, int limit = 10)
    {
        try
        {
            // Lấy flashcards từ module đầu tiên (có thể cải thiện sau)
            var modulesResult = await _flashCardService.GetFlashCardsByModuleIdAsync(1, userId);
            if (!modulesResult.Success || modulesResult.Data == null)
            {
                return new ServiceResponse<List<FlashCardDto>>
                {
                    Success = false,
                    Message = "Không thể tải flashcards"
                };
            }

            // Lọc ra những card chưa có review hoặc review cũ
            var reviewedCardIds = (await _reviewRepo.GetReviewsByUserAsync(userId, 1, 1000))
                .Select(r => r.FlashCardId)
                .ToHashSet();

            var newCards = modulesResult.Data
                .Where(card => !reviewedCardIds.Contains(card.FlashCardId))
                .Take(limit)
                .Select(card => new FlashCardDto
                {
                    FlashCardId = card.FlashCardId,
                    ModuleId = card.ModuleId,
                    Word = card.Word,
                    Meaning = card.Meaning,
                    Pronunciation = card.Pronunciation,
                    CreatedAt = card.CreatedAt,
                    UpdatedAt = card.CreatedAt,
                    ReviewCount = card.ReviewCount,
                    SuccessRate = card.SuccessRate,
                    CurrentLevel = card.CurrentLevel
                })
                .ToList();

            return new ServiceResponse<List<FlashCardDto>>
            {
                Success = true,
                Data = newCards,
                Message = $"Tìm thấy {newCards.Count} từ mới"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new cards for user {UserId}", userId);
            return new ServiceResponse<List<FlashCardDto>>
            {
                Success = false,
                Message = "Không thể tải từ mới"
            };
        }
    }

    public async Task<ServiceResponse<VocabularyReviewDto>> StartReviewAsync(int userId, int flashCardId)
    {
        try
        {
            // Kiểm tra flashcard có tồn tại không
            var cardResult = await _flashCardService.GetFlashCardByIdAsync(flashCardId, userId);
            if (!cardResult.Success || cardResult.Data == null)
            {
                return new ServiceResponse<VocabularyReviewDto>
                {
                    Success = false,
                    Message = "Flashcard không tồn tại"
                };
            }

            // Lấy hoặc tạo review record
            var review = await _reviewRepo.GetReviewAsync(userId, flashCardId);
            if (review == null)
            {
                // Tạo review mới cho từ mới
                review = new FlashCardReview
                {
                    UserId = userId,
                    FlashCardId = flashCardId,
                    Quality = 0,
                    EasinessFactor = 2.5f,
                    IntervalDays = 1,
                    RepetitionCount = 0,
                    NextReviewDate = DateTime.UtcNow,
                    ReviewedAt = DateTime.UtcNow
                };
                review = await _reviewRepo.CreateAsync(review);
            }

            var reviewDto = new VocabularyReviewDto
            {
                ReviewId = review.FlashCardReviewId,
                FlashCardId = review.FlashCardId,
                FlashCard = cardResult.Data,
                Quality = review.Quality,
                EasinessFactor = review.EasinessFactor,
                IntervalDays = review.IntervalDays,
                RepetitionCount = review.RepetitionCount,
                NextReviewDate = review.NextReviewDate,
                ReviewedAt = review.ReviewedAt,
                ReviewStatus = GetReviewStatus(review)
            };

            return new ServiceResponse<VocabularyReviewDto>
            {
                Success = true,
                Data = reviewDto,
                Message = "Bắt đầu ôn tập thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting review for user {UserId}, card {FlashCardId}", userId, flashCardId);
            return new ServiceResponse<VocabularyReviewDto>
            {
                Success = false,
                Message = "Không thể bắt đầu ôn tập"
            };
        }
    }

    public async Task<ServiceResponse<VocabularyReviewResultDto>> SubmitReviewAsync(int reviewId, int quality)
    {
        try
        {
            if (quality < 0 || quality > 5)
            {
                return new ServiceResponse<VocabularyReviewResultDto>
                {
                    Success = false,
                    Message = "Quality phải từ 0-5"
                };
            }

            var review = await _reviewRepo.GetByIdAsync(reviewId);
            if (review == null)
            {
                return new ServiceResponse<VocabularyReviewResultDto>
                {
                    Success = false,
                    Message = "Review không tồn tại"
                };
            }

            // Áp dụng SM-2 Algorithm
            var oldEasiness = review.EasinessFactor;
            var oldInterval = review.IntervalDays;

            // Tính toán easiness factor mới
            var newEasiness = Math.Max(1.3f, oldEasiness + (0.1f - (5 - quality) * (0.08f + (5 - quality) * 0.02f)));

            // Tính toán interval mới
            int newInterval;
            int newRepetitionCount;

            if (quality >= 3) // Nhớ được
            {
                if (review.RepetitionCount == 0)
                {
                    newInterval = 1;
                }
                else if (review.RepetitionCount == 1)
                {
                    newInterval = 6;
                }
                else
                {
                    newInterval = (int)Math.Round(oldInterval * newEasiness);
                }
                newRepetitionCount = review.RepetitionCount + 1;
            }
            else // Quên
            {
                newInterval = 1;
                newRepetitionCount = 0;
            }

            // Cập nhật review
            review.Quality = quality;
            review.EasinessFactor = newEasiness;
            review.IntervalDays = newInterval;
            review.RepetitionCount = newRepetitionCount;
            review.NextReviewDate = DateTime.UtcNow.AddDays(newInterval);
            review.ReviewedAt = DateTime.UtcNow;

            await _reviewRepo.UpdateAsync(review);

            // Cập nhật streak
            await _streakService.UpdateStreakAsync(review.UserId, quality >= 3);

            var result = new VocabularyReviewResultDto
            {
                Success = true,
                Message = "Đã cập nhật kết quả ôn tập",
                NextReviewDate = review.NextReviewDate,
                NewIntervalDays = newInterval,
                NewEasinessFactor = newEasiness,
                ReviewStatus = GetReviewStatus(review)
            };

            return new ServiceResponse<VocabularyReviewResultDto>
            {
                Success = true,
                Data = result,
                Message = "Cập nhật kết quả thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting review {ReviewId} with quality {Quality}", reviewId, quality);
            return new ServiceResponse<VocabularyReviewResultDto>
            {
                Success = false,
                Message = "Không thể lưu kết quả ôn tập"
            };
        }
    }

    public async Task<ServiceResponse<VocabularyStatsDto>> GetVocabularyStatsAsync(int userId)
    {
        try
        {
            var dueCount = await _reviewRepo.GetDueCountAsync(userId, DateTime.UtcNow);
            var totalReviews = await _reviewRepo.GetTotalReviewsCountAsync(userId);
            var masteredCount = await _reviewRepo.GetMasteredCardsCountAsync(userId);

            // Lấy flashcards từ module đầu tiên để tính total cards
            var cardsResult = await _flashCardService.GetFlashCardsByModuleIdAsync(1, userId);
            var totalCards = cardsResult.Success && cardsResult.Data != null ? cardsResult.Data.Count : 0;

            // Tính average quality từ recent reviews
            var recentReviews = await _reviewRepo.GetRecentReviewsAsync(userId, 30);
            var averageQuality = recentReviews.Any() ? recentReviews.Average(r => r.Quality) : 0;

            // Lấy streak info
            var streakResult = await _streakService.GetCurrentStreakAsync(userId);
            var currentStreak = streakResult.Success && streakResult.Data != null ? streakResult.Data.CurrentStreak : 0;

            var stats = new VocabularyStatsDto
            {
                TotalCards = totalCards,
                DueToday = dueCount,
                MasteredCards = masteredCount,
                LearningCards = Math.Max(0, totalReviews - masteredCount),
                NewCards = Math.Max(0, totalCards - totalReviews),
                TotalReviews = totalReviews,
                AverageQuality = Math.Round(averageQuality, 2),
                StreakDays = currentStreak,
                LastReviewDate = recentReviews.FirstOrDefault()?.ReviewedAt
            };

            return new ServiceResponse<VocabularyStatsDto>
            {
                Success = true,
                Data = stats,
                Message = "Lấy thống kê thành công"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vocabulary stats for user {UserId}", userId);
            return new ServiceResponse<VocabularyStatsDto>
            {
                Success = false,
                Message = "Không thể tải thống kê"
            };
        }
    }

    public async Task<ServiceResponse<List<VocabularyReviewDto>>> GetRecentReviewsAsync(int userId, int days = 7)
    {
        try
        {
            var reviews = await _reviewRepo.GetRecentReviewsAsync(userId, days);

            var reviewDtos = reviews.Select(r => new VocabularyReviewDto
            {
                ReviewId = r.FlashCardReviewId,
                FlashCardId = r.FlashCardId,
                FlashCard = MapToFlashCardDto(r.FlashCard),
                Quality = r.Quality,
                EasinessFactor = r.EasinessFactor,
                IntervalDays = r.IntervalDays,
                RepetitionCount = r.RepetitionCount,
                NextReviewDate = r.NextReviewDate,
                ReviewedAt = r.ReviewedAt,
                ReviewStatus = GetReviewStatus(r)
            }).ToList();

            return new ServiceResponse<List<VocabularyReviewDto>>
            {
                Success = true,
                Data = reviewDtos,
                Message = $"Tìm thấy {reviewDtos.Count} review gần đây"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent reviews for user {UserId}", userId);
            return new ServiceResponse<List<VocabularyReviewDto>>
            {
                Success = false,
                Message = "Không thể tải lịch sử ôn tập"
            };
        }
    }

    public async Task<ServiceResponse<bool>> ResetCardProgressAsync(int userId, int flashCardId)
    {
        try
        {
            var review = await _reviewRepo.GetReviewAsync(userId, flashCardId);
            if (review == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Review không tồn tại"
                };
            }

            // Reset về trạng thái ban đầu
            review.Quality = 0;
            review.EasinessFactor = 2.5f;
            review.IntervalDays = 1;
            review.RepetitionCount = 0;
            review.NextReviewDate = DateTime.UtcNow;
            review.ReviewedAt = DateTime.UtcNow;

            await _reviewRepo.UpdateAsync(review);

            return new ServiceResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Đã reset tiến độ từ vựng"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting card progress for user {UserId}, card {FlashCardId}", userId, flashCardId);
            return new ServiceResponse<bool>
            {
                Success = false,
                Message = "Không thể reset tiến độ"
            };
        }
    }

    // Helper methods
    private FlashCardDto MapToFlashCardDto(FlashCard flashCard)
    {
        var dto = new FlashCardDto
        {
            FlashCardId = flashCard.FlashCardId,
            ModuleId = flashCard.ModuleId,
            Word = flashCard.Word,
            Meaning = flashCard.Meaning,
            Pronunciation = flashCard.Pronunciation,
            ImageUrl = flashCard.ImageKey,
            AudioUrl = flashCard.AudioKey,
            PartOfSpeech = flashCard.PartOfSpeech,
            Example = flashCard.Example,
            ExampleTranslation = flashCard.ExampleTranslation,
            Synonyms = flashCard.Synonyms,
            Antonyms = flashCard.Antonyms,
            CreatedAt = flashCard.CreatedAt,
            UpdatedAt = flashCard.UpdatedAt,
            ReviewCount = 0, // TODO: Calculate from reviews
            SuccessRate = 0, // TODO: Calculate from reviews
            LastReviewedAt = null,
            NextReviewAt = null,
            CurrentLevel = 0
        };

        // Convert MinIO keys to public URLs
        if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
        {
            dto.ImageUrl = BuildPublicUrl.BuildURL("flashcards", dto.ImageUrl);
        }
        if (!string.IsNullOrWhiteSpace(dto.AudioUrl))
        {
            dto.AudioUrl = BuildPublicUrl.BuildURL("flashcards", dto.AudioUrl);
        }

        return dto;
    }

    private string GetReviewStatus(FlashCardReview review)
    {
        if (review.RepetitionCount == 0) return "New";
        if (review.IntervalDays < 7) return "Learning";
        if (review.IntervalDays < 21) return "Reviewing";
        return "Mastered";
    }
}
