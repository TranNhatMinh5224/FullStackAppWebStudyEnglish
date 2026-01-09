using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface.Services.Essay;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher
{
    [Route("api/teacher/essays")]
    [ApiController]
    [RequireTeacherRole]
    public class TeacherEssayController : ControllerBase
    {
        private readonly ITeacherEssayService _essayService;
        private readonly ILogger<TeacherEssayController> _logger;

        public TeacherEssayController(ITeacherEssayService essayService, ILogger<TeacherEssayController> logger)
        {
            _essayService = essayService;
            _logger = logger;
        }

        // POST: api/teacher/essays - Teacher tạo essay
        [HttpPost]
        public async Task<IActionResult> CreateEssay([FromBody] CreateEssayDto createDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang tạo essay cho assessment {AssessmentId}", teacherId, createDto.AssessmentId);

            var result = await _essayService.TeacherCreateEssay(createDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetEssay), new { essayId = result.Data?.EssayId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/essays/{essayId} - Teacher lấy essay theo ID
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem essay {EssayId}", teacherId, essayId);

            var result = await _essayService.GetEssayByIdAsync(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/essays/assessment/{assessmentId} - Teacher lấy danh sách essay theo assessment
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xem danh sách essays của assessment {AssessmentId}", teacherId, assessmentId);

            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/essays/{essayId} - Teacher cập nhật essay
        [HttpPut("{essayId}")]
        public async Task<IActionResult> UpdateEssay(int essayId, [FromBody] UpdateEssayDto updateDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang cập nhật essay {EssayId}", teacherId, essayId);

            var result = await _essayService.UpdateEssay(essayId, updateDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/teacher/essays/{essayId} - Teacher xóa essay
        [HttpDelete("{essayId}")]
        public async Task<IActionResult> DeleteEssay(int essayId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa essay {EssayId}", teacherId, essayId);

            var result = await _essayService.DeleteEssay(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
