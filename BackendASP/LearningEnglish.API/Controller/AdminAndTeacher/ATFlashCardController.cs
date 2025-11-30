using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/Flash-Card/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class ATFlashCardController : ControllerBase
    {
        private readonly IFlashCardService _flashCardService;
        private readonly ILogger<ATFlashCardController> _logger;

        public ATFlashCardController(
            IFlashCardService flashCardService,
            ILogger<ATFlashCardController> logger)
        {
            _flashCardService = flashCardService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)!.Value;
        }

        // GET: api/flash-card/atflashcard/{id} - Get flashcard details by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> GetFlashCard(int id)
        {
            var result = await _flashCardService.GetFlashCardByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/flash-card/atflashcard/{moduleId} - Get all flashcards in a module
        [HttpGet("/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> GetFlashCardsByModule(int moduleId)
        {
            var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/flash-card/atflashcard/search - Search flashcards by keyword with optional module filter
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

            var result = await _flashCardService.SearchFlashCardsAsync(searchTerm, moduleId);
            return Ok(result);
        }

        // POST: api/flash-card/atflashcard - Create a new flashcard
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> CreateFlashCard(
            [FromBody] CreateFlashCardDto createFlashCardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ServiceResponse<FlashCardDto>
                {
                    Success = false,
                    Message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}"
                });
            }

            var userId = GetCurrentUserId();
            var result = await _flashCardService.CreateFlashCardAsync(createFlashCardDto, userId);
            return result.Success 
                ? CreatedAtAction(nameof(GetFlashCard), new { id = result.Data!.FlashCardId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/flash-card/atflashcard/{id} - Update flashcard (Admin: any, Teacher: own only)
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> UpdateFlashCard(
            int id,
            [FromBody] UpdateFlashCardDto updateFlashCardDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ServiceResponse<FlashCardDto>
                {
                    Success = false,
                    Message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}"
                });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _flashCardService.UpdateFlashCardAsync(id, updateFlashCardDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/flash-card/atflashcard/{id} - Delete flashcard (Admin: any, Teacher: own only)
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteFlashCard(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _flashCardService.DeleteFlashCardAsync(id, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/flash-card/atflashcard/bulk - Create multiple flashcards at once
        [HttpPost("bulk")]
        public async Task<ActionResult<ServiceResponse<List<FlashCardDto>>>> CreateBulkFlashCards(
            [FromBody] BulkImportFlashCardDto bulkImportDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ServiceResponse<List<FlashCardDto>>
                {
                    Success = false,
                    Message = $"Dữ liệu không hợp lệ: {string.Join(", ", errors)}"
                });
            }

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _flashCardService.CreateBulkFlashCardsAsync(bulkImportDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/flash-card/atflashcard/validate-word - Check if word already exists in module (TODO: implement validation)
        [HttpGet("validate-word")]
        public async Task<ActionResult<ServiceResponse<bool>>> ValidateWord(
            [FromQuery] string word,
            [FromQuery] int moduleId,
            [FromQuery] int? excludeFlashCardId = null)
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                return BadRequest(new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Từ vựng không được để trống"
                });
            }

            // TODO: Add word validation method to repository/service
            var result = new ServiceResponse<bool>
            {
                Data = true,
                Message = "Từ vựng hợp lệ"
            };

            await Task.CompletedTask;
            return Ok(result);
        }
    }
}
