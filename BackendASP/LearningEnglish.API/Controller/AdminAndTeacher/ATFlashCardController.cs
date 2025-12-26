using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/flashcards")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
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

        // POST: api/flash-card/atflashcard - tạo mới flash card
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo flashcard cho modules của own courses
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> CreateFlashCard(
            [FromBody] CreateFlashCardDto createFlashCardDto)
        {
            var userId = User.GetUserId();
            var result = await _flashCardService.CreateFlashCardAsync(createFlashCardDto, userId);
            return result.Success
                ? CreatedAtAction(nameof(GetFlashCard), new { id = result.Data!.FlashCardId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: api/flash-card/atflashcard/{id} - sửa flash card
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ sửa flashcard của own courses (RLS check)
        [HttpPut("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> UpdateFlashCard(
            int id,
            [FromBody] UpdateFlashCardDto updateFlashCardDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _flashCardService.UpdateFlashCardAsync(id, updateFlashCardDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/flash-card/atflashcard/{id} - xoá flash card
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ xóa flashcard của own courses (RLS check)
        [HttpDelete("{id}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteFlashCard(int id)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _flashCardService.DeleteFlashCardAsync(id, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/flash-card/atflashcard/bulk - tạo nhiều flash card từ file excel
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo flashcard cho modules của own courses
        [HttpPost("bulk")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<ServiceResponse<List<FlashCardDto>>>> CreateBulkFlashCards(
            [FromBody] BulkImportFlashCardDto bulkImportDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            var result = await _flashCardService.CreateBulkFlashCardsAsync(bulkImportDto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
