using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/assessments")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class AdminAssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<AdminAssessmentController> _logger;

        public AdminAssessmentController(IAssessmentService assessmentService, ILogger<AdminAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }



        // POST: api/AdminAndTeacher/Assessment/AdminAssessment/create - tạo assessment mới
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo assessment cho modules của own courses
        [HttpPost("create")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            _logger.LogInformation("{UserRole} {UserId} đang tạo Assessment mới: {Title}", userRole, userId, createAssessmentDto.Title);

            int? teacherId = userRole == "Teacher" ? userId : null;
            var result = await _assessmentService.CreateAssessment(createAssessmentDto, teacherId);

            if (!result.Success)
                _logger.LogWarning("Tạo Assessment thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo Assessment thành công với ID: {AssessmentId}", result.Data?.AssessmentId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/AdminAndTeacher/Assessment/AdminAssessment/module/{moduleId} - lấy danh sách assessment theo module ID (Admin và Teacher)
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            _logger.LogInformation("{UserRole} {UserId} đang lấy danh sách Assessment cho Module {ModuleId}", userRole, userId, moduleId);

            var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);

            if (!result.Success)
                _logger.LogWarning("Lấy danh sách Assessment thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Lấy danh sách Assessment thành công cho Module {ModuleId}, tổng số: {Count}", moduleId, result.Data?.Count ?? 0);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/AdminAndTeacher/Assessment/AdminAssessment/{assessmentId} - lay thông tin assessment theo ID (Admin và Teacher)
        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            _logger.LogInformation("{UserRole} {UserId} đang lấy thông tin Assessment {AssessmentId}", userRole, userId, assessmentId);

            var result = await _assessmentService.GetAssessmentById(assessmentId);

            _logger.LogInformation("Lấy thông tin Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/AdminAndTeacher/Assessment/AdminAssessment/{assessmentId} - sửa assessment
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ sửa assessment của own courses (RLS check)
        [HttpPut("{assessmentId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateAssessment(int assessmentId, [FromBody] UpdateAssessmentDto updateAssessmentDto)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            _logger.LogInformation("{UserRole} {UserId} đang cập nhật Assessment {AssessmentId}", userRole, userId, assessmentId);

            var result = await _assessmentService.UpdateAssessment(assessmentId, updateAssessmentDto);

            _logger.LogInformation("Cập nhật Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/AdminAndTeacher/Assessment/AdminAssessment/{assessmentId} - xoá assessment
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ xóa assessment của own courses (RLS check)
        [HttpDelete("{assessmentId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteAssessment(int assessmentId)
        {
            var userId = User.GetUserId();
            var userRole = User.GetPrimaryRole();
            _logger.LogInformation("{UserRole} {UserId} đang xóa Assessment {AssessmentId}", userRole, userId, assessmentId);

            int? teacherId = userRole == "Teacher" ? userId : null;
            var result = await _assessmentService.DeleteAssessment(assessmentId, teacherId);

            _logger.LogInformation("Xóa Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
