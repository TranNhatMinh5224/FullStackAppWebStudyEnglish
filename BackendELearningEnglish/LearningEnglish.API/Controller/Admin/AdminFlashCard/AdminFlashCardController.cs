using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin
{
    [Route("api/admin/flashcards")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin")]
    public class AdminFlashCardController : ControllerBase
    {
        private readonly IAdminFlashCardService _flashCardService;
        private readonly ILogger<AdminFlashCardController> _logger;

        public AdminFlashCardController(IAdminFlashCardService flashCardService, ILogger<AdminFlashCardController> logger)
        {
            _flashCardService = flashCardService;
            _logger = logger;
        }

        // POST: api/admin/flashcards - Admin tạo flashcard
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateFlashCard([FromBody] CreateFlashCardDto createFlashCardDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo flashcard cho module {ModuleId}", adminId, createFlashCardDto.ModuleId);

            var result = await _flashCardService.AdminCreateFlashCard(createFlashCardDto);
            return result.Success
                ? CreatedAtAction(nameof(GetFlashCard), new { flashCardId = result.Data?.FlashCardId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/flashcards/bulk - Admin tạo nhiều flashcards
        [HttpPost("bulk")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> BulkCreateFlashCards([FromBody] BulkImportFlashCardDto bulkImportDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang bulk create flashcards cho module {ModuleId}", adminId, bulkImportDto.ModuleId);

            var result = await _flashCardService.AdminBulkCreateFlashCards(bulkImportDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/flashcards/{flashCardId} - Admin xem chi tiết flashcard
        [HttpGet("{flashCardId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetFlashCard(int flashCardId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem flashcard {FlashCardId}", adminId, flashCardId);

            var result = await _flashCardService.GetFlashCardByIdAsync(flashCardId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/flashcards/module/{moduleId} - Admin xem danh sách flashcards theo module
        [HttpGet("module/{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetFlashCardsByModule(int moduleId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách flashcards của module {ModuleId}", adminId, moduleId);

            var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/flashcards/{flashCardId} - Admin cập nhật flashcard
        [HttpPut("{flashCardId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateFlashCard(int flashCardId, [FromBody] UpdateFlashCardDto updateFlashCardDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật flashcard {FlashCardId}", adminId, flashCardId);

            var result = await _flashCardService.UpdateFlashCard(flashCardId, updateFlashCardDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/flashcards/{flashCardId} - Admin xóa flashcard
        [HttpDelete("{flashCardId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteFlashCard(int flashCardId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa flashcard {FlashCardId}", adminId, flashCardId);

            var result = await _flashCardService.DeleteFlashCard(flashCardId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
