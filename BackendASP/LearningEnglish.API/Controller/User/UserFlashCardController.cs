using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    [Authorize(Roles = "Student")]
    public class UserFlashCardController : ControllerBase
    {
        private readonly IFlashCardService _flashCardService;
        private readonly ILogger<UserFlashCardController> _logger;

        public UserFlashCardController(
            IFlashCardService flashCardService,
            ILogger<UserFlashCardController> logger)
        {
            _flashCardService = flashCardService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        // GET: api/user/flashcard/{id} - Retrieve a specific flashcard by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> GetFlashCard(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _flashCardService.GetFlashCardByIdAsync(id, userId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        // GET: api/user/flashcard/module/{moduleId} - Get all flashcards within a specific module
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> GetFlashCardsByModule(int moduleId)
        {
            var userId = GetCurrentUserId();
            var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }


        // GET: api/user/flashcard/search - Search flashcards by keyword with optional module filter
        [HttpGet("search")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> SearchFlashCards(
            [FromQuery] string searchTerm,
            [FromQuery] int? moduleId = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new ServiceResponse<List<ListFlashCardDto>>
                {
                    Success = false,
                    Message = "Từ khóa tìm kiếm không được để trống"
                });
            }

            var userId = GetCurrentUserId();
            var result = await _flashCardService.SearchFlashCardsAsync(searchTerm, moduleId, userId);
            return Ok(result);
        }

        // GET: api/user/flashcard/progress/{moduleId} - Get flashcard learning progress for a module (TODO: implementation pending)
        [HttpGet("progress/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<FlashCardWithProgressDto>>>> GetFlashCardProgress(int moduleId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang lấy tiến trình FlashCard Module: {ModuleId}", userId, moduleId);

            // TODO: Implement progress tracking
            var result = new ServiceResponse<List<FlashCardWithProgressDto>>
            {
                Data = new List<FlashCardWithProgressDto>(),
                Message = "Lấy tiến trình học FlashCard thành công"
            };

            await Task.CompletedTask;
            return Ok(result);
        }

        // POST: api/user/flashcard/reset-progress/{flashCardId} - Reset learning progress for a specific flashcard (TODO: implementation pending)
        [HttpPost("reset-progress/{flashCardId}")]
        public async Task<ActionResult<ServiceResponse<bool>>> ResetFlashCardProgress(int flashCardId)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} đang reset progress FlashCard: {FlashCardId}", userId, flashCardId);

            // TODO: Implement progress reset
            var result = new ServiceResponse<bool>
            {
                Data = true,
                Message = "Reset tiến trình FlashCard thành công"
            };

            await Task.CompletedTask;
            return Ok(result);
        }
    }
}
