using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // bắt đầu học module mới
        // RLS: flashcardreviews_policy_user_all_own
        [HttpPost("start-module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<int>>> StartLearningModule(int moduleId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang bắt đầu học module {ModuleId}", userId, moduleId);

            var result = await _reviewService.StartLearningModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // lấy danh sách từ cần ôn tập
        // RLS: flashcardreviews_policy_user_all_own (chỉ xem reviews của chính mình)
        [HttpGet("due")]
        public async Task<ActionResult<ServiceResponse<DueFlashCardsResponseDto>>> GetDueFlashCards()
        {
            // RLS sẽ filter flashcard reviews theo userId
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang lấy danh sách từ cần ôn tập", userId);

            var result = await _reviewService.GetDueFlashCardsAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // review flashcard
        // RLS: flashcardreviews_policy_user_all_own (chỉ update reviews của chính mình)
        // FluentValidation: ReviewFlashCardDto validator sẽ tự động validate
        [HttpPost("review")]
        public async Task<ActionResult<ServiceResponse<ReviewFlashCardResponseDto>>> ReviewFlashCard([FromBody] ReviewFlashCardDto reviewDto)
        {
            // RLS sẽ filter flashcard reviews theo userId
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang review flashcard {FlashCardId} với quality {Quality}",
                userId, reviewDto.FlashCardId, reviewDto.Quality);

            var result = await _reviewService.ReviewFlashCardAsync(userId, reviewDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // lấy thống kê review
        // RLS: flashcardreviews_policy_user_all_own (chỉ xem reviews của chính mình)
        [HttpGet("statistics")]
        public async Task<ActionResult<ServiceResponse<ReviewStatisticsDto>>> GetStatistics()
        {
            // RLS sẽ filter flashcard reviews theo userId
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang lấy thống kê review", userId);

            var result = await _reviewService.GetReviewStatisticsAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // lấy danh sách từ đã thuộc
        // RLS: flashcardreviews_policy_user_all_own (chỉ xem reviews của chính mình)
        [HttpGet("mastered")]
        public async Task<ActionResult<ServiceResponse<DueFlashCardsResponseDto>>> GetMasteredFlashCards()
        {
            // RLS sẽ filter flashcard reviews theo userId
            var userId = User.GetUserId();
            _logger.LogInformation("User {UserId} đang lấy danh sách từ đã thuộc", userId);

            var result = await _reviewService.GetMasteredFlashCardsAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
