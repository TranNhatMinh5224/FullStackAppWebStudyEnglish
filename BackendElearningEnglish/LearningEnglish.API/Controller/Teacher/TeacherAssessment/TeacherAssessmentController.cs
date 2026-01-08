using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher.TeacherAssessment
{
    [Route("api/teacher/assessments")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherAssessmentController : ControllerBase
    {
        private readonly ITeacherAssessmentService _assessmentService;
        private readonly ILogger<TeacherAssessmentController> _logger;

        public TeacherAssessmentController(ITeacherAssessmentService assessmentService, ILogger<TeacherAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        // POST: api/teacher/assessments - tạo assessment mới (chỉ cho modules của own courses)
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] CreateAssessmentDto createAssessmentDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo Assessment mới: {Title}", teacherId, createAssessmentDto.Title);

            var result = await _assessmentService.CreateAssessmentAsync(createAssessmentDto, teacherId);

            if (!result.Success)
                _logger.LogWarning("Tạo Assessment thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Tạo Assessment thành công với ID: {AssessmentId}", result.Data?.AssessmentId);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/assessments/module/{moduleId} - lấy danh sách assessment theo module ID (chỉ own courses)
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy danh sách Assessment cho Module {ModuleId}", teacherId, moduleId);

            var result = await _assessmentService.GetAssessmentsByModuleIdAsync(moduleId, teacherId);

            if (!result.Success)
                _logger.LogWarning("Lấy danh sách Assessment thất bại: {Message}", result.Message);
            else
                _logger.LogInformation("Lấy danh sách Assessment thành công cho Module {ModuleId}, tổng số: {Count}", moduleId, result.Data?.Count ?? 0);

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/assessments/{assessmentId} - lấy thông tin assessment theo ID (chỉ own courses)
        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang lấy thông tin Assessment {AssessmentId}", teacherId, assessmentId);

            var result = await _assessmentService.GetAssessmentByIdAsync(assessmentId, teacherId);

            _logger.LogInformation("Lấy thông tin Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/assessments/{assessmentId} - cập nhật assessment (chỉ own courses)
        [HttpPut("{assessmentId}")]
        public async Task<IActionResult> UpdateAssessment(int assessmentId, [FromBody] UpdateAssessmentDto updateAssessmentDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật Assessment {AssessmentId}", teacherId, assessmentId);

            var result = await _assessmentService.UpdateAssessmentAsync(assessmentId, updateAssessmentDto, teacherId);

            _logger.LogInformation("Cập nhật Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/assessments/{assessmentId} - xóa assessment (chỉ own courses)
        [HttpDelete("{assessmentId}")]
        public async Task<IActionResult> DeleteAssessment(int assessmentId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa Assessment {AssessmentId}", teacherId, assessmentId);

            var result = await _assessmentService.DeleteAssessmentAsync(assessmentId, teacherId);

            _logger.LogInformation("Xóa Assessment thành công: {AssessmentId}", assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
