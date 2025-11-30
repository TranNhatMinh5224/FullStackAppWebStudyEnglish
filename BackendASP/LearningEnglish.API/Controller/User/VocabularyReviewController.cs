using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User;

[ApiController]
[Route("api/user/[controller]")]
[Authorize(Roles = "Student")]
public class VocabularyReviewController : ControllerBase
{
    private readonly IVocabularyReviewService _vocabularyService;
    private readonly ILogger<VocabularyReviewController> _logger;

    public VocabularyReviewController(
        IVocabularyReviewService vocabularyService,
        ILogger<VocabularyReviewController> logger)
    {
        _vocabularyService = vocabularyService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    }

    // GET: api/user/vocabularyreview/due - Get all flashcards due for review today using SM-2 algorithm
    [HttpGet("due")]
    public async Task<IActionResult> GetDueReviews()
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyService.GetDueReviewsAsync(userId);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // GET: api/user/vocabularyreview/new - Get new flashcards to learn (default limit: 10)
    [HttpGet("new")]
    public async Task<IActionResult> GetNewCards([FromQuery] int limit = 10)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyService.GetNewCardsAsync(userId, limit);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // POST: api/user/vocabularyreview/start/{flashCardId} - Start a review session for a specific flashcard
    [HttpPost("start/{flashCardId}")]
    public async Task<IActionResult> StartReview(int flashCardId)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyService.StartReviewAsync(userId, flashCardId);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // POST: api/user/vocabularyreview/submit/{reviewId} - Submit review result with quality score (0-5) for SM-2 algorithm
    [HttpPost("submit/{reviewId}")]
    public async Task<IActionResult> SubmitReview(int reviewId, [FromBody] SubmitReviewRequestDto request)
    {
        if (request.Quality < 0 || request.Quality > 5)
        {
            return BadRequest(new ServiceResponse<VocabularyReviewResultDto>
            {
                Success = false,
                Message = "Quality phải từ 0-5"
            });
        }

        var result = await _vocabularyService.SubmitReviewAsync(reviewId, request.Quality);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // GET: api/user/vocabularyreview/stats - Get vocabulary learning statistics (cards learned, retention rate, etc.)
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyService.GetVocabularyStatsAsync(userId);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // GET: api/user/vocabularyreview/recent - Get recent review history (default: last 7 days)
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentReviews([FromQuery] int days = 7)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyService.GetRecentReviewsAsync(userId, days);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }

    // POST: api/user/vocabularyreview/reset/{flashCardId} - Reset learning progress for a card (when completely forgotten)
    [HttpPost("reset/{flashCardId}")]
    public async Task<IActionResult> ResetCardProgress(int flashCardId)
    {
        var userId = GetCurrentUserId();
        var result = await _vocabularyService.ResetCardProgressAsync(userId, flashCardId);
        return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
    }
}
