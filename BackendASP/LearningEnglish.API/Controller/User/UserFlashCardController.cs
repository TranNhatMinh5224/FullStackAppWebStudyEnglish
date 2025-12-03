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
    }
}
