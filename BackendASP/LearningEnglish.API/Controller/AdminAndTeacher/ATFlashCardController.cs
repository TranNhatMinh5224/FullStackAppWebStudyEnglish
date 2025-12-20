using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/flashcards")]
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
            return User.GetPrimaryRole();
        }

        // GET: api/flash-card/atflashcard/{id} - lấy flashcard theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> GetFlashCard(int id)
        {
            var result = await _flashCardService.GetFlashCardByIdAsync(id);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/flash-card/atflashcard/{moduleId} - lấy tất cả flash card
        [HttpGet("/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> GetFlashCardsByModule(int moduleId)
        {
            var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/flashcards/module/{moduleId}/paginated?pageNumber=1&pageSize=20 - lấy danh sách flash card phân trang
        [HttpGet("module/{moduleId}/paginated")]
        public async Task<ActionResult<ServiceResponse<PagedResult<ListFlashCardDto>>>> GetFlashCardsByModulePaginated(
            int moduleId,
            [FromQuery] PageRequest request)
        {
            var result = await _flashCardService.GetFlashCardsByModuleIdPaginatedAsync(moduleId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/flash-card/atflashcard - tạo mơis flash card
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

        // PUT: api/flash-card/atflashcard/{id} - sửa lại flash card, teacher chỉ sửa của riêng teacher, còn admin sửa tất cả
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

        // DELETE: api/flash-card/atflashcard/{id} - xoá flash card, teacher chỉ xoá của riêng teacher, còn admin xoá tất cả
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteFlashCard(int id)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _flashCardService.DeleteFlashCardAsync(id, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/flash-card/atflashcard/bulk - tạo nhiều flash card từ file excel
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
    }
}
