using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/[controller]")]
    [Authorize(Roles = "User")]
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

        // GET: api/user/flashcard/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceResponse<FlashCardDto>>> GetFlashCard(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                
                _logger.LogInformation("User {UserId} đang xem FlashCard: {FlashCardId}", userId, id);

                var result = await _flashCardService.GetFlashCardByIdAsync(id, userId);

                if (!result.Success)
                {
                    return NotFound(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User lấy FlashCard: {FlashCardId}", id);
                return StatusCode(500, new ServiceResponse<FlashCardDto>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin FlashCard"
                });
            }
        }

        // GET: api/user/flashcard/module/{moduleId}
        [HttpGet("module/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<ListFlashCardDto>>>> GetFlashCardsByModule(int moduleId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                _logger.LogInformation("User {UserId} đang xem FlashCard trong Module: {ModuleId}", userId, moduleId);

                var result = await _flashCardService.GetFlashCardsByModuleIdAsync(moduleId, userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User lấy danh sách FlashCard trong Module: {ModuleId}", moduleId);
                return StatusCode(500, new ServiceResponse<List<ListFlashCardDto>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách FlashCard"
                });
            }
        }



        // GET: api/user/flashcard/search
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

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                _logger.LogInformation("User {UserId} đang tìm kiếm FlashCard với từ khóa: {SearchTerm}", userId, searchTerm);

                var result = await _flashCardService.SearchFlashCardsAsync(searchTerm, moduleId, userId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User tìm kiếm FlashCard: {SearchTerm}", searchTerm);
                return StatusCode(500, new ServiceResponse<List<ListFlashCardDto>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi tìm kiếm FlashCard"
                });
            }
        }



        // GET: api/user/flashcard/progress/{moduleId}
        [HttpGet("progress/{moduleId}")]
        public async Task<ActionResult<ServiceResponse<List<FlashCardWithProgressDto>>>> GetFlashCardProgress(int moduleId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                _logger.LogInformation("User {UserId} đang lấy tiến trình FlashCard Module: {ModuleId}", userId, moduleId);

                // TODO: Implement progress tracking
                var result = new ServiceResponse<List<FlashCardWithProgressDto>>
                {
                    Data = new List<FlashCardWithProgressDto>(),
                    Message = "Lấy tiến trình học FlashCard thành công"
                };

                await Task.CompletedTask; // Giữ async signature cho tương lai

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User lấy tiến trình FlashCard Module: {ModuleId}", moduleId);
                return StatusCode(500, new ServiceResponse<List<FlashCardWithProgressDto>>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi lấy tiến trình FlashCard"
                });
            }
        }

        // POST: api/user/flashcard/reset-progress/{flashCardId}
        [HttpPost("reset-progress/{flashCardId}")]
        public async Task<ActionResult<ServiceResponse<bool>>> ResetFlashCardProgress(int flashCardId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                _logger.LogInformation("User {UserId} đang reset progress FlashCard: {FlashCardId}", userId, flashCardId);

                // TODO: Implement progress reset
                var result = new ServiceResponse<bool>
                {
                    Data = true,
                    Message = "Reset tiến trình FlashCard thành công"
                };

                await Task.CompletedTask; // Giữ async signature cho tương lai

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User reset progress FlashCard: {FlashCardId}", flashCardId);
                return StatusCode(500, new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "Có lỗi xảy ra khi reset tiến trình FlashCard"
                });
            }
        }


    }
}
