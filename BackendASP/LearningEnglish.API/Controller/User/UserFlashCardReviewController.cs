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

    
        /// POST: api/user/flashcard-review/review - Submit a flashcard review with quality rating (0-5)
       
        [HttpPost("review")]
        public async Task<ActionResult<ServiceResponse<ReviewFlashCardResponseDto>>> ReviewFlashCard([FromBody] ReviewFlashCardDto reviewDto)
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("User {UserId} đang review flashcard {FlashCardId} với quality {Quality}", 
                userId, reviewDto.FlashCardId, reviewDto.Quality);

            var result = await _reviewService.ReviewFlashCardAsync(userId, reviewDto);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// GET: api/user/flashcard-review/due - Get all flashcards due for review today
        /// Optional query param: ?moduleId=123
        [HttpGet("due")]
        public async Task<ActionResult<ServiceResponse<DueFlashCardsResponseDto>>> GetDueFlashCards([FromQuery] int? moduleId = null)
        {
            var userId = GetCurrentUserId();
            
            if (moduleId.HasValue)
            {
                _logger.LogInformation("User {UserId} đang lấy từ cần ôn tập trong module {ModuleId}", userId, moduleId.Value);
                var result = await _reviewService.GetDueFlashCardsByModuleAsync(userId, moduleId.Value);
                return Ok(result);
            }
            else
            {
                _logger.LogInformation("User {UserId} đang lấy danh sách từ cần ôn tập", userId);
                var result = await _reviewService.GetDueFlashCardsAsync(userId);
                return Ok(result);
            }
        }

       
        /// GET: api/user/flashcard-review/due/count - Get count of flashcards due today
    
        [HttpGet("due/count")]
        public async Task<ActionResult<ServiceResponse<int>>> GetDueCount()
        {
            var userId = GetCurrentUserId();
            
            var result = await _reviewService.GetDueCountAsync(userId);
            
            return Ok(result);
        }

    
        /// GET: api/user/flashcard-review/statistics - Get comprehensive review statistics
  
        [HttpGet("statistics")]
        public async Task<ActionResult<ServiceResponse<ReviewStatisticsDto>>> GetStatistics()
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("User {UserId} đang lấy thống kê review", userId);

            var result = await _reviewService.GetReviewStatisticsAsync(userId);
            
            return Ok(result);
        }

  
        /// POST: api/user/flashcard-review/start-module/{moduleId} - Start learning a module (add all flashcards to review system)
     
        [HttpPost("start-module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<int>>> StartLearningModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("User {UserId} đang bắt đầu học module {ModuleId}", userId, moduleId);

            var result = await _reviewService.StartLearningModuleAsync(userId, moduleId);
            
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

   
        /// GET: api/user/flashcard-review/mastered - Get all mastered flashcards (won't be reviewed anymore)
      
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
