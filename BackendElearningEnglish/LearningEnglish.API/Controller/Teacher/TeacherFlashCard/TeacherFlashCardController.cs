using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher
{
    [Route("api/teacher/flashcards")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherFlashCardController : ControllerBase
    {
        private readonly ITeacherFlashCardCommandService _flashCardCommandService;
        private readonly ITeacherFlashCardQueryService _flashCardQueryService;
        private readonly ILogger<TeacherFlashCardController> _logger;

        public TeacherFlashCardController(
            ITeacherFlashCardCommandService flashCardCommandService,
            ITeacherFlashCardQueryService flashCardQueryService,
            ILogger<TeacherFlashCardController> logger)
        {
            _flashCardCommandService = flashCardCommandService;
            _flashCardQueryService = flashCardQueryService;
            _logger = logger;
        }

        // POST: api/teacher/flashcards - Teacher tạo flashcard
        [HttpPost]
        public async Task<IActionResult> CreateFlashCard([FromBody] CreateFlashCardDto createFlashCardDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo flashcard cho module {ModuleId}", teacherId, createFlashCardDto.ModuleId);

            var result = await _flashCardCommandService.TeacherCreateFlashCard(createFlashCardDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetFlashCard), new { flashCardId = result.Data?.FlashCardId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/flashcards/bulk - Teacher tạo nhiều flashcards
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreateFlashCards([FromBody] BulkImportFlashCardDto bulkImportDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang bulk create flashcards cho module {ModuleId}", teacherId, bulkImportDto.ModuleId);

            var result = await _flashCardCommandService.TeacherBulkCreateFlashCards(bulkImportDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/flashcards/{flashCardId} - Teacher xem chi tiết flashcard
        [HttpGet("{flashCardId}")]
        public async Task<IActionResult> GetFlashCard(int flashCardId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem flashcard {FlashCardId}", teacherId, flashCardId);

            var result = await _flashCardQueryService.GetFlashCardByIdAsync(flashCardId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/flashcards/module/{moduleId} - Teacher xem danh sách flashcards theo module
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetFlashCardsByModule(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách flashcards của module {ModuleId}", teacherId, moduleId);

            var result = await _flashCardQueryService.GetFlashCardsByModuleIdAsync(moduleId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/flashcards/{flashCardId} - Teacher cập nhật flashcard
        [HttpPut("{flashCardId}")]
        public async Task<IActionResult> UpdateFlashCard(int flashCardId, [FromBody] UpdateFlashCardDto updateFlashCardDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật flashcard {FlashCardId}", teacherId, flashCardId);

            var result = await _flashCardCommandService.UpdateFlashCard(flashCardId, updateFlashCardDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/flashcards/{flashCardId} - Teacher xóa flashcard
        [HttpDelete("{flashCardId}")]
        public async Task<IActionResult> DeleteFlashCard(int flashCardId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa flashcard {FlashCardId}", teacherId, flashCardId);

            var result = await _flashCardCommandService.DeleteFlashCard(flashCardId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
