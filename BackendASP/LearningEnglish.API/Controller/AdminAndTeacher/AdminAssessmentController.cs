using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/AdminAndTeacher/Assessment/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class AdminAssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AdminAssessmentController> _logger;

        public AdminAssessmentController(IAssessmentService assessmentService, ILogger<AdminAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }


        // Tạo Assessment mới (Admin và Teacher)
        [HttpPost("create")]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                _logger.LogInformation("{UserRole} {UserId} đang tạo Assessment mới: {Title}", userRole, GetCurrentUserId(), createAssessmentDto.Title);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

                var result = await _assessmentService.CreateAssessment(createAssessmentDto, teacherId);

                if (!result.Success)
                {
                    _logger.LogWarning("Tạo Assessment thất bại: {Message}", result.Message);
                    return StatusCode(result.StatusCode, result);
                }

                _logger.LogInformation("Tạo Assessment thành công với ID: {AssessmentId}", result.Data?.AssessmentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Assessment");
                return StatusCode(500, "Có lỗi xảy ra khi tạo Assessment");
            }
        }

        // Lấy danh sách Assessment theo ModuleId (Admin và Teacher)
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                _logger.LogInformation("{UserRole} {UserId} đang lấy danh sách Assessment cho Module {ModuleId}", userRole, GetCurrentUserId(), moduleId);

                var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);

                if (!result.Success)
                {
                    _logger.LogWarning("Lấy danh sách Assessment thất bại: {Message}", result.Message);
                    return StatusCode(result.StatusCode, result);
                }

                _logger.LogInformation("Lấy danh sách Assessment thành công cho Module {ModuleId}, tổng số: {Count}", moduleId, result.Data?.Count ?? 0);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Assessment cho Module {ModuleId}", moduleId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy danh sách Assessment");
            }
        }

        // Lấy thông tin Assessment theo ID (Admin và Teacher)
        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                _logger.LogInformation("{UserRole} {UserId} đang lấy thông tin Assessment {AssessmentId}", userRole, GetCurrentUserId(), assessmentId);

                var result = await _assessmentService.GetAssessmentById(assessmentId);


                _logger.LogInformation("Lấy thông tin Assessment thành công: {AssessmentId}", assessmentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin Assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Có lỗi xảy ra khi lấy thông tin Assessment");
            }
        }

        // Cập nhật Assessment (Admin và Teacher)
        [HttpPut("{assessmentId}")]
        public async Task<IActionResult> UpdateAssessment(int assessmentId, [FromBody] UpdateAssessmentDto updateAssessmentDto)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                _logger.LogInformation("{UserRole} {UserId} đang cập nhật Assessment {AssessmentId}", userRole, GetCurrentUserId(), assessmentId);

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _assessmentService.UpdateAssessment(assessmentId, updateAssessmentDto);



                _logger.LogInformation("Cập nhật Assessment thành công: {AssessmentId}", assessmentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Có lỗi xảy ra khi cập nhật Assessment");
            }
        }

        // Xóa Assessment (Admin và Teacher)
        [HttpDelete("{assessmentId}")]
        public async Task<IActionResult> DeleteAssessment(int assessmentId)
        {
            try
            {
                var userRole = GetCurrentUserRole();
                var currentUserId = GetCurrentUserId();
                _logger.LogInformation("{UserRole} {UserId} đang xóa Assessment {AssessmentId}", userRole, currentUserId, assessmentId);

                int? teacherId = userRole == "Teacher" ? currentUserId : null;

                var result = await _assessmentService.DeleteAssessment(assessmentId, teacherId);

                _logger.LogInformation("Xóa Assessment thành công: {AssessmentId}", assessmentId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "Có lỗi xảy ra khi xóa Assessment");
            }
        }
    }
}
