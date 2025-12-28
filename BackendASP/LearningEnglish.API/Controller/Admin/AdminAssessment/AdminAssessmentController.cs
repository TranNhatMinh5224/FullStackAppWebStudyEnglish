using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin.AdminAssessment
{
    [Route("api/admin/assessments")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin")]
    public class AdminAssessmentController : ControllerBase
    {
        private readonly IAdminAssessmentService _assessmentService;
        private readonly ILogger<AdminAssessmentController> _logger;

        public AdminAssessmentController(IAdminAssessmentService assessmentService, ILogger<AdminAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        // POST: api/admin/assessments - tạo assessment mới
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang tạo Assessment mới: {Title}", userId, createAssessmentDto.Title);

            var result = await _assessmentService.CreateAssessmentAsync(createAssessmentDto);

           
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/assessments/module/{moduleId} - lấy danh sách assessment theo module ID
        [HttpGet("module/{moduleId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang lấy danh sách Assessment cho Module {ModuleId}", userId, moduleId);

            var result = await _assessmentService.GetAssessmentsByModuleIdAsync(moduleId);


            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/assessments/{assessmentId} - lấy thông tin assessment theo ID
        [HttpGet("{assessmentId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang lấy thông tin Assessment {AssessmentId}", userId, assessmentId);

            var result = await _assessmentService.GetAssessmentByIdAsync(assessmentId);

            _logger.LogInformation("Lấy thông tin Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/admin/assessments/{assessmentId} - cập nhật assessment
        [HttpPut("{assessmentId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> UpdateAssessment(int assessmentId, [FromBody] UpdateAssessmentDto updateAssessmentDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang cập nhật Assessment {AssessmentId}", userId, assessmentId);

            var result = await _assessmentService.UpdateAssessmentAsync(assessmentId, updateAssessmentDto);

            _logger.LogInformation("Cập nhật Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/admin/assessments/{assessmentId} - xóa assessment
        [HttpDelete("{assessmentId}")]
        [RequirePermission("Admin.Content.Manage")]
        public async Task<IActionResult> DeleteAssessment(int assessmentId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang xóa Assessment {AssessmentId}", userId, assessmentId);

            var result = await _assessmentService.DeleteAssessmentAsync(assessmentId);

            _logger.LogInformation("Xóa Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
