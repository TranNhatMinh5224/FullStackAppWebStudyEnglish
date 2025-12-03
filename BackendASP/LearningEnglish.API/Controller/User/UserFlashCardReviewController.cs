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

        // ============================================================
        // SECTION 1: BẮT ĐẦU HỌC (Start Learning)
        // ============================================================

        /// <summary>
        /// POST: api/user/flashcard-review/start-module/{moduleId}
        /// Bắt đầu học module mới - Thêm tất cả từ vựng trong module vào "rổ review"
        /// </summary>
        [HttpPost("start-module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<int>>> StartLearningModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang bắt đầu học module {ModuleId}", userId, moduleId);

            var result = await _reviewService.StartLearningModuleAsync(userId, moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // ============================================================
        // SECTION 2: LẤY TỪ CẦN ÔN HÔM NAY (Get Due Cards)
        // ============================================================

        /// <summary>
        /// GET: api/user/flashcard-review/due
        /// Lấy danh sách TẤT CẢ từ cần ôn hôm nay (từ mọi modules đã học)
        /// Đây là "rổ review chung" - không phân biệt module
        /// </summary>
        [HttpGet("due")]
        public async Task<ActionResult<ServiceResponse<DueFlashCardsResponseDto>>> GetDueFlashCards()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy danh sách từ cần ôn tập", userId);
            
            var result = await _reviewService.GetDueFlashCardsAsync(userId);
            return Ok(result);
        }

        // ============================================================
        // SECTION 3: ÔN TẬ/HỌC TỪ (Review Cards)
        // ============================================================

        /// <summary>
        /// POST: api/user/flashcard-review/review
        /// Ôn tập/Học từ - Submit đánh giá quality (0-5)
        /// Hệ thống tự động tính toán lần ôn tiếp theo theo SM-2 algorithm
        /// </summary>
        [HttpPost("review")]
        public async Task<ActionResult<ServiceResponse<ReviewFlashCardResponseDto>>> ReviewFlashCard([FromBody] ReviewFlashCardDto reviewDto)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang review flashcard {FlashCardId} với quality {Quality}", 
                userId, reviewDto.FlashCardId, reviewDto.Quality);

            var result = await _reviewService.ReviewFlashCardAsync(userId, reviewDto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ============================================================
        // SECTION 4: THỐNG KÊ & TIẾN ĐỘ (Statistics & Progress)
        // ============================================================

        /// <summary>
        /// GET: api/user/flashcard-review/statistics
        /// Lấy thống kê tổng quan (dashboard)
        /// Bao gồm: Tổng số từ, từ đã thuộc, từ cần ôn, success rate, streak...
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<ServiceResponse<ReviewStatisticsDto>>> GetStatistics()
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy thống kê review", userId);

            var result = await _reviewService.GetReviewStatisticsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// GET: api/user/flashcard-review/mastered
        /// Lấy danh sách từ đã thuộc (mastered)
        /// Từ không cần ôn nữa (RepetitionCount >= 5, Quality >= 4)
        /// </summary>
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
