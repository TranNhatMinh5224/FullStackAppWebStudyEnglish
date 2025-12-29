using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LearningEnglish.Infrastructure.Repositories;

public class FlashCardReviewRepository : IFlashCardReviewRepository
{
    private readonly AppDbContext _context;

    public FlashCardReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FlashCardReview?> GetByIdAsync(int reviewId)
    {
        return await _context.FlashCardReviews
            .Include(r => r.FlashCard)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.FlashCardReviewId == reviewId);
    }

    public async Task<FlashCardReview?> GetReviewAsync(int userId, int flashCardId)
    {
        return await _context.FlashCardReviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.FlashCardId == flashCardId);
    }

    public async Task<List<FlashCardReview>> GetDueReviewsAsync(int userId, DateTime currentDate)
    {
        return await _context.FlashCardReviews
            .Include(r => r.FlashCard)
            .Where(r => r.UserId == userId && r.NextReviewDate <= currentDate)
            .OrderBy(r => r.NextReviewDate)
            .ToListAsync();
    }

    public async Task<List<FlashCardReview>> GetReviewsByUserAsync(int userId, int page = 1, int pageSize = 20)
    {
        return await _context.FlashCardReviews
            .Include(r => r.FlashCard)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReviewedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetDueCountAsync(int userId, DateTime currentDate)
    {
        return await _context.FlashCardReviews
            .CountAsync(r => r.UserId == userId && r.NextReviewDate <= currentDate);
    }

    public async Task<int> GetTotalReviewsCountAsync(int userId)
    {
        return await _context.FlashCardReviews.CountAsync(r => r.UserId == userId);
    }

    public async Task<int> GetMasteredCardsCountAsync(int userId)
    {
        return await _context.FlashCardReviews
            .CountAsync(r => r.UserId == userId && r.NextReviewDate == DateTime.MaxValue);
    }

    public async Task<FlashCardReview> CreateAsync(FlashCardReview review)
    {
        await _context.FlashCardReviews.AddAsync(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task UpdateAsync(FlashCardReview review)
    {
        _context.FlashCardReviews.Update(review);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int reviewId)
    {
        var review = await _context.FlashCardReviews.FindAsync(reviewId);
        if (review != null)
        {
            _context.FlashCardReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<FlashCardReview>> GetRecentReviewsAsync(int userId, int days = 7)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        return await _context.FlashCardReviews
            .Include(r => r.FlashCard)
            .Where(r => r.UserId == userId && r.ReviewedAt >= startDate)
            .OrderByDescending(r => r.ReviewedAt)
            .ToListAsync();
    }
}
