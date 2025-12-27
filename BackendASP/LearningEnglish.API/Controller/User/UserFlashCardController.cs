using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/flashcards")]
    [Authorize(Roles = "Student")]
    public class UserFlashCardController : ControllerBase
    {
        private readonly IUserFlashCardService _flashCardService;
        private readonly ILogger<UserFlashCardController> _logger;

        public UserFlashCardController(
            IUserFlashCardService flashCardService,
            ILogger<UserFlashCardController> logger)
        {
            _flashCardService = flashCardService;
            _logger = logger;
        }

        // GET: api/user/flashcard/{id} - lấy flashcard theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> GetFlashCard(int id)
        {
            var userId = User.GetUserId();
            var result = await _flashCardService.GetFlashCardByIdAsync(id, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/flashcard/module/{moduleId} - lấy tất cả flash card theo module ID
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> GetFlashCardsByModule(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

    }
}
