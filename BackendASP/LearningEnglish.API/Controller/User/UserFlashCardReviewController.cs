using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/flashcard-review")]
    [Authorize(Roles = "Student")]
    public class UserFlashCardReviewController : ControllerBase
    {
        private readonly IFlashCardReviewService _reviewService;
        private readonly ILogger<UserFlashCardReviewController> _logger;

        public UserFlashCardReviewController(
            IFlashCardReviewService reviewService,
            ILogger<UserFlashCardReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        // bắt đầu học module mới
        [HttpPost("start-module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<int>>> StartLearningModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang bắt đầu học module {ModuleId}", userId, moduleId);

            var result = await _reviewService.StartLearningModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // lấy danh sách từ cần ôn tập
        [HttpGet("due")]
        public async Task<ActionResult<ServiceResponse<DueFlashCardsResponseDto>>> GetDueFlashCards()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy danh sách từ cần ôn tập", userId);

            var result = await _reviewService.GetDueFlashCardsAsync(userId);
            return Ok(result);
        }

        // review flashcard
        [HttpPost("review")]
        public async Task<ActionResult<ServiceResponse<ReviewFlashCardResponseDto>>> ReviewFlashCard([FromBody] ReviewFlashCardDto reviewDto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang review flashcard {FlashCardId} với quality {Quality}",
                userId, reviewDto.FlashCardId, reviewDto.Quality);

            var result = await _reviewService.ReviewFlashCardAsync(userId, reviewDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // lấy thống kê review
        [HttpGet("statistics")]
        public async Task<ActionResult<ServiceResponse<ReviewStatisticsDto>>> GetStatistics()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy thống kê review", userId);

            var result = await _reviewService.GetReviewStatisticsAsync(userId);
            return Ok(result);
        }

        // lấy danh sách từ đã thuộc
        [HttpGet("mastered")]
        public async Task<ActionResult<ServiceResponse<DueFlashCardsResponseDto>>> GetMasteredFlashCards()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy danh sách từ đã thuộc", userId);

            var result = await _reviewService.GetMasteredFlashCardsAsync(userId);
            return Ok(result);
        }
    }
}
