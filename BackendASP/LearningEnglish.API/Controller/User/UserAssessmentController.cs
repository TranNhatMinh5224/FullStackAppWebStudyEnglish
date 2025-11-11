using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/Assessment")]
    [ApiController]
    [Authorize]
    public class UserAssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<UserAssessmentController> _logger;

        public UserAssessmentController(IAssessmentService assessmentService, ILogger<UserAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // Học viên lấy danh sách Assessment theo ModuleId
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} đang lấy danh sách Assessment cho Module {ModuleId}", userId, moduleId);

                var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);

                if (!result.Success)
                {
                    _logger.LogWarning("Lấy danh sách Assessment thất bại cho User {UserId}: {Message}", userId, result.Message);
                    return StatusCode(result.StatusCode, result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User {UserId} lấy danh sách Assessment cho Module {ModuleId}", GetCurrentUserId(), moduleId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy danh sách Assessment");
            }
        }

        // Học viên lấy thông tin Assessment theo ID
        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("User {UserId} đang lấy thông tin Assessment {AssessmentId}", userId, assessmentId);

                var result = await _assessmentService.GetAssessmentById(assessmentId);

                if (!result.Success)
                {
                    _logger.LogWarning("Lấy thông tin Assessment thất bại cho User {UserId}: {Message}", userId, result.Message);
                    return StatusCode(result.StatusCode, result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi User {UserId} lấy thông tin Assessment {AssessmentId}", GetCurrentUserId(), assessmentId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy thông tin Assessment");
            }
        }
    }
}
