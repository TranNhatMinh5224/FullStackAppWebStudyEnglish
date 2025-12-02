using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.Shared
{
    [ApiController]
    [Route("api/flashcards")]
    [Authorize]
    public class FlashCardController : ControllerBase
    {
        private readonly IFlashCardService _flashCardService;

        public FlashCardController(IFlashCardService flashCardService)
        {
            _flashCardService = flashCardService;
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        // GET: api/flashcards/search - Search flashcards (shared for all roles)
        // Students: Only see flashcards from enrolled modules
        // Teachers/Admin: See all flashcards or filter by module
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
    }
}
