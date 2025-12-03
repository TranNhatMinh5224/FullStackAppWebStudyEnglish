using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class FlashCardReviewService : IFlashCardReviewService
    {
        private readonly IFlashCardReviewRepository _reviewRepository;
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly IMapper _mapper;
        private readonly ILogger<FlashCardReviewService> _logger;
        private readonly IStreakService _streakService;

        // MinIO bucket constants
        private const string AUDIO_BUCKET_NAME = "flashcard-audio";
        private const string IMAGE_BUCKET_NAME = "flashcards";

        public FlashCardReviewService(
            IFlashCardReviewRepository reviewRepository,
            IFlashCardRepository flashCardRepository,
            IModuleProgressService moduleProgressService,
            IMapper mapper,
            ILogger<FlashCardReviewService> logger,
            IStreakService streakService)
        {
            _reviewRepository = reviewRepository;
            _flashCardRepository = flashCardRepository;
            _moduleProgressService = moduleProgressService;
            _mapper = mapper;
            _logger = logger;
            _streakService = streakService;
        }

        public async Task<ServiceResponse<ReviewFlashCardResponseDto>> ReviewFlashCardAsync(int userId, ReviewFlashCardDto reviewDto)
        {
            var response = new ServiceResponse<ReviewFlashCardResponseDto>();

            try
            {
                // Validate quality (0-5)
                if (reviewDto.Quality < 0 || reviewDto.Quality > 5)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Quality phải từ 0-5";
                    return response;
                }

                // Check if flashcard exists
                var flashCard = await _flashCardRepository.GetByIdAsync(reviewDto.FlashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard";
                    return response;
                }

                // Get or create review record
                var existingReview = await _reviewRepository.GetReviewAsync(userId, reviewDto.FlashCardId);
                FlashCardReview review;

                if (existingReview == null)
                {
                    // First time reviewing this card
                    review = new FlashCardReview
                    {
                        UserId = userId,
                        FlashCardId = reviewDto.FlashCardId,
                        Quality = reviewDto.Quality,
                        ReviewedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    review = existingReview;
                    review.Quality = reviewDto.Quality;
                    review.ReviewedAt = DateTime.UtcNow;
                }

                // Apply SM-2 Spaced Repetition Algorithm
                CalculateNextReview(review);

                // Save to database
                if (existingReview == null)
                {
                    await _reviewRepository.CreateAsync(review);
                }
                else
                {
                    await _reviewRepository.UpdateAsync(review);
                }

                // Build response
                var responseData = new ReviewFlashCardResponseDto
                {
                    FlashCardReviewId = review.FlashCardReviewId,
                    FlashCardId = review.FlashCardId,
                    Word = flashCard.Word,
                    Quality = review.Quality,
                    EasinessFactor = review.EasinessFactor,
                    IntervalDays = review.IntervalDays,
                    RepetitionCount = review.RepetitionCount,
                    NextReviewDate = review.NextReviewDate,
                    Message = GetReviewMessage(review.Quality, review.IntervalDays)
                };

                response.Data = responseData;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Review flashcard thành công";

                _logger.LogInformation("User {UserId} reviewed flashcard {FlashCardId} with quality {Quality}. Next review: {NextReview}",
                    userId, reviewDto.FlashCardId, reviewDto.Quality, review.NextReviewDate);

                // ✅ Update streak after flashcard review
                bool isSuccessful = reviewDto.Quality >= 3; // Quality >= 3 is considered successful
                await _streakService.UpdateStreakAsync(userId, isSuccessful);

                // ✅ Check if module is completed after this review
                if (flashCard.ModuleId.HasValue)
                {
                    await CheckAndCompleteModuleAsync(userId, flashCard.ModuleId.Value);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reviewing flashcard for user {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi review flashcard: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<DueFlashCardsResponseDto>> GetDueFlashCardsAsync(int userId)
        {
            var response = new ServiceResponse<DueFlashCardsResponseDto>();

            try
            {
                var currentDate = DateTime.UtcNow.Date;
                _logger.LogInformation("GetDueFlashCardsAsync - UserId: {UserId}, CurrentDate: {CurrentDate}", userId, currentDate);
                
                var dueReviews = await _reviewRepository.GetDueReviewsAsync(userId, currentDate);
                _logger.LogInformation("Found {Count} due reviews for user {UserId}", dueReviews.Count(), userId);

                var dueFlashCards = new List<DueFlashCardDto>();

                foreach (var review in dueReviews)
                {
                    var flashCard = review.FlashCard;
                    
                    // Generate URLs from keys
                    string? imageUrl = null;
                    string? audioUrl = null;

                    if (!string.IsNullOrEmpty(flashCard.ImageKey))
                    {
                        imageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCard.ImageKey);
                    }

                    if (!string.IsNullOrEmpty(flashCard.AudioKey))
                    {
                        audioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCard.AudioKey);
                    }

                    var daysOverdue = (currentDate - review.NextReviewDate.Date).Days;

                    dueFlashCards.Add(new DueFlashCardDto
                    {
                        FlashCardId = flashCard.FlashCardId,
                        ModuleId = flashCard.ModuleId,
                        Word = flashCard.Word,
                        Meaning = flashCard.Meaning,
                        Pronunciation = flashCard.Pronunciation,
                        ImageUrl = imageUrl,
                        AudioUrl = audioUrl,
                        PartOfSpeech = flashCard.PartOfSpeech,
                        Example = flashCard.Example,
                        ExampleTranslation = flashCard.ExampleTranslation,
                        NextReviewDate = review.NextReviewDate,
                        IntervalDays = review.IntervalDays,
                        RepetitionCount = review.RepetitionCount,
                        IsOverdue = daysOverdue > 0,
                        DaysOverdue = daysOverdue > 0 ? daysOverdue : 0
                    });
                }

                var responseData = new DueFlashCardsResponseDto
                {
                    TotalDue = dueFlashCards.Count,
                    NewCards = dueFlashCards.Count(c => c.RepetitionCount == 0),
                    ReviewCards = dueFlashCards.Count(c => c.RepetitionCount > 0),
                    OverdueCards = dueFlashCards.Count(c => c.IsOverdue),
                    FlashCards = dueFlashCards.OrderBy(c => c.NextReviewDate).ToList()
                };

                response.Data = responseData;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = $"Có {responseData.TotalDue} từ cần ôn tập hôm nay";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting due flashcards for user {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy danh sách từ cần ôn: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<ReviewStatisticsDto>> GetReviewStatisticsAsync(int userId)
        {
            var response = new ServiceResponse<ReviewStatisticsDto>();

            try
            {
                var currentDate = DateTime.UtcNow.Date;
                var allReviews = await _reviewRepository.GetReviewsByUserAsync(userId, 1, 10000);
                var recentReviews = await _reviewRepository.GetRecentReviewsAsync(userId, 7);
                var dueCount = await _reviewRepository.GetDueCountAsync(userId, currentDate);
                var masteredCount = await _reviewRepository.GetMasteredCardsCountAsync(userId);

                var todayReviews = allReviews.Where(r => r.ReviewedAt.Date == currentDate).ToList();
                var weekReviews = allReviews.Where(r => r.ReviewedAt.Date >= currentDate.AddDays(-7)).ToList();

                // Calculate success rate (quality >= 3 is considered successful)
                var totalReviewsCount = allReviews.Count;
                var successfulReviews = allReviews.Count(r => r.Quality >= 3);
                var successRate = totalReviewsCount > 0 ? (decimal)successfulReviews / totalReviewsCount * 100 : 0;

                // Calculate average quality
                var avgQuality = totalReviewsCount > 0 ? (decimal)allReviews.Average(r => r.Quality) : 0;

                // Upcoming reviews (next 7 days)
                var upcomingReviews = new Dictionary<string, int>();
                for (int i = 1; i <= 7; i++)
                {
                    var date = currentDate.AddDays(i);
                    var count = allReviews.Count(r => r.NextReviewDate.Date == date);
                    upcomingReviews[date.ToString("yyyy-MM-dd")] = count;
                }

                // Calculate streak (simplified - consecutive days with reviews)
                var currentStreak = CalculateCurrentStreak(allReviews, currentDate);
                var longestStreak = CalculateLongestStreak(allReviews);

                var statistics = new ReviewStatisticsDto
                {
                    TotalCards = totalReviewsCount,
                    DueToday = dueCount,
                    NewToday = todayReviews.Count(r => r.RepetitionCount == 0),
                    ReviewedToday = todayReviews.Count,
                    MasteredCards = masteredCount,
                    ReviewedThisWeek = weekReviews.Count,
                    NewThisWeek = weekReviews.Count(r => r.RepetitionCount == 0),
                    AverageQuality = avgQuality,
                    SuccessRate = successRate,
                    UpcomingReviews = upcomingReviews,
                    CurrentStreak = currentStreak,
                    LongestStreak = longestStreak
                };

                response.Data = statistics;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thống kê thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting review statistics for user {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy thống kê: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<int>> StartLearningModuleAsync(int userId, int moduleId)
        {
            var response = new ServiceResponse<int>();

            try
            {
                // Get all flashcards in module
                var flashCards = await _flashCardRepository.GetByModuleIdAsync(moduleId);

                if (!flashCards.Any())
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Module không có flashcard nào";
                    return response;
                }

                int newCardsAdded = 0;
                int existingCardsCount = 0;

                foreach (var flashCard in flashCards)
                {
                    // Check if already exists
                    var existingReview = await _reviewRepository.GetReviewAsync(userId, flashCard.FlashCardId);

                    if (existingReview == null)
                    {
                        // Create initial review record for new card
                        var newReview = new FlashCardReview
                        {
                            UserId = userId,
                            FlashCardId = flashCard.FlashCardId,
                            Quality = 0,
                            EasinessFactor = 2.5f,
                            IntervalDays = 0,
                            RepetitionCount = 0,
                            NextReviewDate = DateTime.UtcNow.Date, // Due today for first review
                            ReviewedAt = DateTime.UtcNow
                        };

                        await _reviewRepository.CreateAsync(newReview);
                        newCardsAdded++;
                        
                        _logger.LogInformation("Added new flashcard {FlashCardId} to review system for user {UserId}, NextReviewDate: {NextReviewDate}", 
                            flashCard.FlashCardId, userId, newReview.NextReviewDate);
                    }
                    else
                    {
                        existingCardsCount++;
                        _logger.LogDebug("FlashCard {FlashCardId} already exists in review system for user {UserId}", 
                            flashCard.FlashCardId, userId);
                    }
                }

                int totalCards = flashCards.Count;
                response.Data = totalCards;
                response.Success = true;
                response.StatusCode = 200;

                // Build detailed message
                if (newCardsAdded == 0)
                {
                    response.Message = $"Module có {totalCards} từ. Tất cả đã có trong hệ thống ôn tập. Bạn có thể bắt đầu ôn ngay!";
                }
                else if (existingCardsCount == 0)
                {
                    response.Message = $"Đã thêm {newCardsAdded} từ mới vào hệ thống ôn tập. Sẵn sàng để học!";
                }
                else
                {
                    response.Message = $"Module có {totalCards} từ: {newCardsAdded} từ mới được thêm, {existingCardsCount} từ đã có sẵn.";
                }

                _logger.LogInformation("User {UserId} started learning module {ModuleId} - Total: {Total}, New: {New}, Existing: {Existing}", 
                    userId, moduleId, totalCards, newCardsAdded, existingCardsCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting learning module for user {UserId}, module {ModuleId}", userId, moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi bắt đầu học module: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> StartLearningFlashCardAsync(int userId, int flashCardId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Check if already exists
                var existingReview = await _reviewRepository.GetReviewAsync(userId, flashCardId);

                if (existingReview != null)
                {
                    response.Data = false;
                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Từ này đã có trong hệ thống ôn tập";
                    return response;
                }

                // Check if flashcard exists
                var flashCard = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard";
                    return response;
                }

                // Create initial review record
                var newReview = new FlashCardReview
                {
                    UserId = userId,
                    FlashCardId = flashCardId,
                    Quality = 0,
                    EasinessFactor = 2.5f,
                    IntervalDays = 1,
                    RepetitionCount = 0,
                    NextReviewDate = DateTime.UtcNow.Date, // Due today
                    ReviewedAt = DateTime.UtcNow
                };

                await _reviewRepository.CreateAsync(newReview);

                response.Data = true;
                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Đã thêm từ vào hệ thống ôn tập";

                _logger.LogInformation("User {UserId} started learning flashcard {FlashCardId}", userId, flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting learning flashcard for user {UserId}, flashcard {FlashCardId}", userId, flashCardId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi thêm từ vào hệ thống: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<DueFlashCardsResponseDto>> GetMasteredFlashCardsAsync(int userId)
        {
            var response = new ServiceResponse<DueFlashCardsResponseDto>();

            try
            {
                // Get all reviews with NextReviewDate = MaxValue (mastered)
                var allReviews = await _reviewRepository.GetReviewsByUserAsync(userId, 1, 10000);
                var masteredReviews = allReviews.Where(r => r.NextReviewDate == DateTime.MaxValue).ToList();

                var masteredFlashCards = new List<DueFlashCardDto>();

                foreach (var review in masteredReviews)
                {
                    var flashCard = review.FlashCard;
                    
                    // Generate URLs from keys
                    string? imageUrl = null;
                    string? audioUrl = null;

                    if (!string.IsNullOrEmpty(flashCard.ImageKey))
                    {
                        imageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCard.ImageKey);
                    }

                    if (!string.IsNullOrEmpty(flashCard.AudioKey))
                    {
                        audioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCard.AudioKey);
                    }

                    masteredFlashCards.Add(new DueFlashCardDto
                    {
                        FlashCardId = flashCard.FlashCardId,
                        ModuleId = flashCard.ModuleId,
                        Word = flashCard.Word,
                        Meaning = flashCard.Meaning,
                        Pronunciation = flashCard.Pronunciation,
                        ImageUrl = imageUrl,
                        AudioUrl = audioUrl,
                        PartOfSpeech = flashCard.PartOfSpeech,
                        Example = flashCard.Example,
                        ExampleTranslation = flashCard.ExampleTranslation,
                        NextReviewDate = review.NextReviewDate,
                        IntervalDays = review.IntervalDays,
                        RepetitionCount = review.RepetitionCount,
                        IsOverdue = false,
                        DaysOverdue = 0
                    });
                }

                var responseData = new DueFlashCardsResponseDto
                {
                    TotalDue = masteredFlashCards.Count,
                    NewCards = 0,
                    ReviewCards = masteredFlashCards.Count,
                    OverdueCards = 0,
                    FlashCards = masteredFlashCards.OrderByDescending(c => c.IntervalDays).ToList()
                };

                response.Data = responseData;
                response.Success = true;
                response.StatusCode = 200;
                response.Message = $"Bạn đã thuộc {responseData.TotalDue} từ vựng! 🎉";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting mastered flashcards for user {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy danh sách từ đã thuộc: {ex.Message}";
            }

            return response;
        }

        #region Private Helper Methods

        /// <summary>
        /// SM-2 Spaced Repetition Algorithm
        /// Reference: https://en.wikipedia.org/wiki/SuperMemo#SM-2_algorithm
        /// </summary>
        private void CalculateNextReview(FlashCardReview review)
        {
            var quality = review.Quality;

            // Update Easiness Factor
            // EF' = EF + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02))
            var newEF = review.EasinessFactor + (0.1f - (5 - quality) * (0.08f + (5 - quality) * 0.02f));
            
            // EF should not be less than 1.3
            review.EasinessFactor = Math.Max(1.3f, newEF);

            if (quality < 3)
            {
                // Failed review - restart
                review.RepetitionCount = 0;
                review.IntervalDays = 1;
            }
            else
            {
                // Successful review
                review.RepetitionCount++;

                if (review.RepetitionCount == 1)
                {
                    review.IntervalDays = 1;
                }
                else if (review.RepetitionCount == 2)
                {
                    review.IntervalDays = 6;
                }
                else
                {
                    // interval(n) = interval(n-1) * EF
                    review.IntervalDays = (int)Math.Ceiling(review.IntervalDays * review.EasinessFactor);
                }
            }

            // Check if mastered: IntervalDays >= 60 (2 months) AND RepetitionCount >= 5
            // Once mastered, set NextReviewDate to far future (won't appear in /due)
            if (review.IntervalDays >= 60 && review.RepetitionCount >= 5)
            {
                review.NextReviewDate = DateTime.MaxValue; // Never review again
                _logger.LogInformation("FlashCard {FlashCardId} mastered! IntervalDays: {IntervalDays}, Repetition: {RepetitionCount}",
                    review.FlashCardId, review.IntervalDays, review.RepetitionCount);
            }
            else
            {
                // Calculate next review date
                review.NextReviewDate = DateTime.UtcNow.Date.AddDays(review.IntervalDays);
            }
        }

        private string GetReviewMessage(int quality, int intervalDays)
        {
            return quality switch
            {
                5 => $"Tuyệt vời! Hẹn gặp lại sau {intervalDays} ngày 🎉",
                4 => $"Tốt lắm! Xem lại sau {intervalDays} ngày 👍",
                3 => $"Được đấy! Ôn lại sau {intervalDays} ngày 📚",
                2 => $"Cần cố gắng thêm. Hẹn gặp lại sau {intervalDays} ngày 💪",
                1 => $"Chưa tốt. Ôn lại sau {intervalDays} ngày 📖",
                0 => $"Hãy ôn lại sau {intervalDays} ngày nhé! 🔄",
                _ => $"Hẹn gặp lại sau {intervalDays} ngày"
            };
        }

        private int CalculateCurrentStreak(List<FlashCardReview> reviews, DateTime currentDate)
        {
            if (!reviews.Any()) return 0;

            var orderedDates = reviews
                .Select(r => r.ReviewedAt.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            if (!orderedDates.Any() || orderedDates[0] < currentDate.AddDays(-1))
            {
                return 0; // Streak broken
            }

            int streak = 0;
            var checkDate = currentDate;

            foreach (var date in orderedDates)
            {
                if (date == checkDate || date == checkDate.AddDays(-1))
                {
                    streak++;
                    checkDate = date.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        private int CalculateLongestStreak(List<FlashCardReview> reviews)
        {
            if (!reviews.Any()) return 0;

            var orderedDates = reviews
                .Select(r => r.ReviewedAt.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToList();

            int longestStreak = 1;
            int currentStreak = 1;

            for (int i = 1; i < orderedDates.Count; i++)
            {
                if ((orderedDates[i] - orderedDates[i - 1]).Days == 1)
                {
                    currentStreak++;
                    longestStreak = Math.Max(longestStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1;
                }
            }

            return longestStreak;
        }

        /// <summary>
        /// Check if all flashcards in module are mastered (RepetitionCount >= 5 AND Quality >= 4)
        /// If yes, mark module as completed
        /// </summary>
        private async Task CheckAndCompleteModuleAsync(int userId, int moduleId)
        {
            try
            {
                // Get all flashcards in the module
                var flashCards = await _flashCardRepository.GetByModuleIdAsync(moduleId);
                if (!flashCards.Any()) return;

                // Get all reviews for this user in this module
                var moduleFlashCardIds = flashCards.Select(fc => fc.FlashCardId).ToList();
                var allReviews = await _reviewRepository.GetReviewsByUserAsync(userId);
                var moduleReviews = allReviews.Where(r => moduleFlashCardIds.Contains(r.FlashCardId)).ToList();

                // Check if all flashcards have been reviewed and mastered
                // Mastered criteria: RepetitionCount >= 5 AND last quality >= 4
                bool allMastered = flashCards.All(fc =>
                {
                    var review = moduleReviews.FirstOrDefault(r => r.FlashCardId == fc.FlashCardId);
                    return review != null && review.RepetitionCount >= 5 && review.Quality >= 4;
                });

                if (allMastered)
                {
                    // Mark module as completed
                    await _moduleProgressService.CompleteModuleAsync(userId, moduleId);
                    _logger.LogInformation(
                        "User {UserId} completed FlashCard module {ModuleId} - All {Count} cards mastered",
                        userId, moduleId, flashCards.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking module completion for user {UserId}, module {ModuleId}", userId, moduleId);
                // Don't throw - this is a background check, shouldn't fail the main operation
            }
        }

        /// <summary>
        /// Get count of flashcards due for review today
        /// Used by background services and notifications
        /// </summary>
        public async Task<int> GetDueCountAsync(int userId)
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _reviewRepository.GetDueCountAsync(userId, currentDate);
        }

        #endregion
    }
}
