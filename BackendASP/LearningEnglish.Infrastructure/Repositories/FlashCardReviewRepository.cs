using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Repositories;

public class FlashCardReviewRepository : BaseRepository<FlashCardReview>, IFlashCardReviewRepository
{
    private readonly ILogger<FlashCardReviewRepository> _logger;

    public FlashCardReviewRepository(AppDbContext context, ILogger<FlashCardReviewRepository> logger)
        : base(context)
    {
        _logger = logger;
    }

    public new async Task<FlashCardReview?> GetByIdAsync(int reviewId)
    {
        try
        {
            return await _context.FlashCardReviews
                .Include(r => r.FlashCard)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.FlashCardReviewId == reviewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FlashCardReview by ID: {ReviewId}", reviewId);
            throw;
        }
    }

    public async Task<FlashCardReview?> GetReviewAsync(int userId, int flashCardId)
    {
        try
        {
            return await _context.FlashCardReviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.FlashCardId == flashCardId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting review for user {UserId}, card {FlashCardId}", userId, flashCardId);
            throw;
        }
    }

    public async Task<List<FlashCardReview>> GetDueReviewsAsync(int userId, DateTime currentDate)
    {
        try
        {
            return await _context.FlashCardReviews
                .Include(r => r.FlashCard)
                .Where(r => r.UserId == userId && r.NextReviewDate <= currentDate)
                .OrderBy(r => r.NextReviewDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting due reviews for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<FlashCardReview>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20)
    {
        try
        {
            return await _context.FlashCardReviews
                .Include(r => r.FlashCard)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReviewedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reviews for user {UserId}, page {Page}, size {PageSize}",
                userId, page, pageSize);
            throw;
        }
    }

    public async Task<int> GetDueCountAsync(int userId, DateTime currentDate)
    {
        try
        {
            return await _context.FlashCardReviews
                .CountAsync(r => r.UserId == userId && r.NextReviewDate <= currentDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting due count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetTotalReviewsCountAsync(int userId)
    {
        try
        {
            return await _context.FlashCardReviews.CountAsync(r => r.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total reviews count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<int> GetMasteredCardsCountAsync(int userId)
    {
        try
        {
            // Mastered cards are marked with NextReviewDate = DateTime.MaxValue by the service
            // This approach is flexible and doesn't depend on hard-coded interval values
            return await _context.FlashCardReviews
                .CountAsync(r => r.UserId == userId && r.NextReviewDate == DateTime.MaxValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mastered cards count for user {UserId}", userId);
            throw;
        }
    }

    public async Task<FlashCardReview> CreateAsync(FlashCardReview review)
    {
        try
        {
            return await AddAsync(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating FlashCardReview for user {UserId}, card {FlashCardId}",
                review.UserId, review.FlashCardId);
            throw;
        }
    }

    public new async Task UpdateAsync(FlashCardReview review)
    {
        try
        {
            await base.UpdateAsync(review);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating FlashCardReview {ReviewId}", review.FlashCardReviewId);
            throw;
        }
    }

    public async Task DeleteAsync(int reviewId)
    {
        try
        {
            await DeleteByIdAsync(reviewId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting FlashCardReview {ReviewId}", reviewId);
            throw;
        }
    }

    public async Task<List<FlashCardReview>> GetRecentReviewsAsync(int userId, int days = 7)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            return await _context.FlashCardReviews
                .Include(r => r.FlashCard)
                .Where(r => r.UserId == userId && r.ReviewedAt >= startDate)
                .OrderByDescending(r => r.ReviewedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent reviews for user {UserId}, days {Days}", userId, days);
            throw;
        }
    }
}
