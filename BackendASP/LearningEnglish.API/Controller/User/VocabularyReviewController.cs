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

    /// <summary>
    /// Lấy danh sách từ vựng cần ôn tập hôm nay
    /// </summary>
    [HttpGet("due")]
    public async Task<IActionResult> GetDueReviews()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy danh sách ôn tập", userId);

            var result = await _vocabularyService.GetDueReviewsAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách ôn tập");
            return StatusCode(500, new ServiceResponse<List<VocabularyReviewDto>>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }

    /// <summary>
    /// Lấy từ vựng mới để học
    /// </summary>
    [HttpGet("new")]
    public async Task<IActionResult> GetNewCards([FromQuery] int limit = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy từ mới, limit: {Limit}", userId, limit);

            var result = await _vocabularyService.GetNewCardsAsync(userId, limit);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy từ mới");
            return StatusCode(500, new ServiceResponse<List<FlashCardDto>>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }

    /// <summary>
    /// Bắt đầu ôn tập một từ vựng
    /// </summary>
    [HttpPost("start/{flashCardId}")]
    public async Task<IActionResult> StartReview(int flashCardId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} bắt đầu ôn tập từ {FlashCardId}", userId, flashCardId);

            var result = await _vocabularyService.StartReviewAsync(userId, flashCardId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi bắt đầu ôn tập từ {FlashCardId}", flashCardId);
            return StatusCode(500, new ServiceResponse<VocabularyReviewDto>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }

    /// <summary>
    /// Submit kết quả ôn tập (quality score 0-5)
    /// </summary>
    [HttpPost("submit/{reviewId}")]
    public async Task<IActionResult> SubmitReview(int reviewId, [FromBody] SubmitReviewRequestDto request)
    {
        try
        {
            if (request.Quality < 0 || request.Quality > 5)
            {
                return BadRequest(new ServiceResponse<VocabularyReviewResultDto>
                {
                    Success = false,
                    Message = "Quality phải từ 0-5"
                });
            }

            _logger.LogInformation("Submitting review {ReviewId} with quality {Quality}", reviewId, request.Quality);

            var result = await _vocabularyService.SubmitReviewAsync(reviewId, request.Quality);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi submit review {ReviewId}", reviewId);
            return StatusCode(500, new ServiceResponse<VocabularyReviewResultDto>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }

    /// <summary>
    /// Lấy thống kê vocabulary review
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy thống kê vocabulary", userId);

            var result = await _vocabularyService.GetVocabularyStatsAsync(userId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy thống kê vocabulary");
            return StatusCode(500, new ServiceResponse<VocabularyStatsDto>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }

    /// <summary>
    /// Lấy lịch sử ôn tập gần đây
    /// </summary>
    [HttpGet("recent")]
    public async Task<IActionResult> GetRecentReviews([FromQuery] int days = 7)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy lịch sử ôn tập {Days} ngày", userId, days);

            var result = await _vocabularyService.GetRecentReviewsAsync(userId, days);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử ôn tập");
            return StatusCode(500, new ServiceResponse<List<VocabularyReviewDto>>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }

    /// <summary>
    /// Reset tiến độ của một từ vựng (khi quên hoàn toàn)
    /// </summary>
    [HttpPost("reset/{flashCardId}")]
    public async Task<IActionResult> ResetCardProgress(int flashCardId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} reset tiến độ từ {FlashCardId}", userId, flashCardId);

            var result = await _vocabularyService.ResetCardProgressAsync(userId, flashCardId);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi reset tiến độ từ {FlashCardId}", flashCardId);
            return StatusCode(500, new ServiceResponse<bool>
            {
                Success = false,
                Message = "Lỗi hệ thống"
            });
        }
    }
}
