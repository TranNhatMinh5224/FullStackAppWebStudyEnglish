using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.Essay;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin
{
    [Route("api/admin/essays")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin")]
    public class AdminEssayController : ControllerBase
    {
        private readonly IAdminEssayService _essayService;
        private readonly ILogger<AdminEssayController> _logger;

        public AdminEssayController(IAdminEssayService essayService, ILogger<AdminEssayController> logger)
        {
            _essayService = essayService;
            _logger = logger;
        }

        // POST: api/admin/essays - Admin tạo essay
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateEssay([FromBody] CreateEssayDto createDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang tạo essay cho assessment {AssessmentId}", adminId, createDto.AssessmentId);

            var result = await _essayService.AdminCreateEssay(createDto);
            return result.Success
                ? CreatedAtAction(nameof(GetEssay), new { essayId = result.Data?.EssayId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/essays/{essayId} - Admin lấy essay theo ID
        [HttpGet("{essayId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem essay {EssayId}", adminId, essayId);

            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/essays/assessment/{assessmentId} - Admin lấy danh sách essay theo assessment
        [HttpGet("assessment/{assessmentId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xem danh sách essays của assessment {AssessmentId}", adminId, assessmentId);

            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/essays/{essayId} - Admin cập nhật essay
        [HttpPut("{essayId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateEssay(int essayId, [FromBody] UpdateEssayDto updateDto)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang cập nhật essay {EssayId}", adminId, essayId);

            var result = await _essayService.UpdateEssay(essayId, updateDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/essays/{essayId} - Admin xóa essay
        [HttpDelete("{essayId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteEssay(int essayId)
        {
            var adminId = User.GetUserId();
            _logger.LogInformation("Admin {AdminId} đang xóa essay {EssayId}", adminId, essayId);

            var result = await _essayService.DeleteEssay(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
