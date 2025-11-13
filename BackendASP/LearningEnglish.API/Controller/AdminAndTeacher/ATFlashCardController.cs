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

        // Get FlashCard by ID - Admin/Teacher
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> GetFlashCard(int id)
        {
            try
            {
                _logger.LogInformation("Admin/Teacher đang lấy FlashCard với ID: {FlashCardId}", id);

                var result = await _flashCardService.GetFlashCardByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm FlashCard");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> GetFlashCardsByModule(int moduleId)
        {
            try
            {
                _logger.LogInformation("Admin/Teacher đang lấy danh sách FlashCard trong Module: {ModuleId}", moduleId);

                var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách FlashCard trong Module: {ModuleId}", moduleId);
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpGet("search")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> SearchFlashCards(
            [FromQuery] string searchTerm,
            [FromQuery] int? moduleId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new ServiceResponse<List<ListFlashCardDto>>
                    {
                        Success = false,
                        Message = "Từ khóa tìm kiếm không được để trống"
                    });
                }

                _logger.LogInformation("Admin/Teacher đang tìm kiếm FlashCard với từ khóa: {SearchTerm}", searchTerm);

                var result = await _flashCardService.SearchFlashCardsAsync(searchTerm, moduleId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm FlashCard");
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/at/flashcard
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> CreateFlashCard(
            [FromBody] CreateFlashCardDto createFlashCardDto)
        {
            try
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

                _logger.LogInformation("Admin/Teacher {UserId} đang tạo FlashCard mới: {Word}", userId, createFlashCardDto.Word);

                var result = await _flashCardService.CreateFlashCardAsync(createFlashCardDto, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return CreatedAtAction(nameof(GetFlashCard),
                    new { id = result.Data!.FlashCardId }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Loi khi tạo FlashCard mới");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/at/flashcard/{id}
        [HttpPut("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> UpdateFlashCard(
            int id,
            [FromBody] UpdateFlashCardDto updateFlashCardDto)
        {
            try
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

                _logger.LogInformation("Admin/Teacher {UserId} đang cập nhật FlashCard: {FlashCardId}", userId, id);

                var result = await _flashCardService.UpdateFlashCardWithAuthorizationAsync(
                    id, updateFlashCardDto, userId, userRole);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "loi khi cập nhật FlashCard: {FlashCardId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/at/flashcard/{id}
        [HttpDelete("{id}")]
        public async Task<ActionResult<ServiceResponse<bool>>> DeleteFlashCard(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

                _logger.LogInformation("Admin/Teacher {UserId} đang xóa FlashCard: {FlashCardId}", userId, id);

                var result = await _flashCardService.DeleteFlashCardWithAuthorizationAsync(id, userId, userRole);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "loi khi xoa FlashCard: {FlashCardId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/at/flashcard/bulk
        [HttpPost("bulk")]
        public async Task<ActionResult<ServiceResponse<List<FlashCardDto>>>> CreateBulkFlashCards(
            [FromBody] BulkImportFlashCardDto bulkImportDto)
        {
            try
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var userRole = User.FindFirst(ClaimTypes.Role)!.Value;

                _logger.LogInformation("Admin/Teacher {UserId} đang tạo {Count} FlashCard hàng loạt",
                    userId, bulkImportDto.FlashCards.Count);

                var result = await _flashCardService.CreateBulkFlashCardsAsync(bulkImportDto, userId, userRole);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "loi khi tạo FlashCard hàng loạt");
                return StatusCode(500, "Internal server error");
            }
        }





        // GET: api/at/flashcard/validate-word
        [HttpGet("validate-word")]
        public async Task<ActionResult<ServiceResponse<bool>>> ValidateWord(
            [FromQuery] string word,
            [FromQuery] int moduleId,
            [FromQuery] int? excludeFlashCardId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(word))
                {
                    return BadRequest(new ServiceResponse<bool>
                    {
                        Success = false,
                        Message = "Từ vựng không được để trống"
                    });
                }

                _logger.LogInformation("Kiểm tra từ vựng '{Word}' trong Module: {ModuleId}", word, moduleId);

                // TODO: Add word validation method to repository/service
                var result = new ServiceResponse<bool>
                {
                    Data = true, // Placeholder - implement actual validation
                    Message = "Từ vựng hợp lệ"
                };

                await Task.CompletedTask; // Giữ async signature cho tương lai

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error adding lesson for teacher");
                return StatusCode(500, "Internal server error");
            }

        }
    }
}
