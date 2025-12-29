using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Teacher
{
    [Route("api/teacher/essay-submissions")]
    [ApiController]
    [Authorize]
    [RequireTeacherRole]
    public class TeacherEssaySubmissionController : ControllerBase
    {
        private readonly ITeacherEssaySubmissionService _essaySubmissionService;
        private readonly ITeacherEssayGradingService _gradingService;

        public TeacherEssaySubmissionController(
            ITeacherEssaySubmissionService essaySubmissionService,
            ITeacherEssayGradingService gradingService)
        {
            _essaySubmissionService = essaySubmissionService;
            _gradingService = gradingService;
        }

        // GET: api/teacher/essay-submissions/essay/{essayId} - Lấy danh sách submissions với phân trang
        [HttpGet("essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionsByEssay(
            int essayId,
            [FromQuery] PageRequest request)
        {
            var teacherId = User.GetUserId();
            var result = await _essaySubmissionService.GetSubmissionsByEssayIdPagedAsync(essayId, teacherId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/essay-submissions/{submissionId}
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmissionDetail(int submissionId)
        {
            var teacherId = User.GetUserId();
            var result = await _essaySubmissionService.GetSubmissionDetailAsync(submissionId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/essay-submissions/{submissionId}/download
        [HttpGet("{submissionId}/download")]
        public async Task<IActionResult> DownloadSubmissionFile(int submissionId)
        {
            var teacherId = User.GetUserId();
            var result = await _essaySubmissionService.DownloadSubmissionFileAsync(submissionId, teacherId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            var (fileStream, fileName, contentType) = result.Data;
            return File(fileStream, contentType, fileName);
        }

        // POST: api/teacher/essay-submissions/{submissionId}/grade-ai
        [HttpPost("{submissionId}/grade-ai")]
        public async Task<IActionResult> GradeWithAI(int submissionId)
        {
            var teacherId = User.GetUserId();
            var result = await _gradingService.GradeEssayWithAIAsync(submissionId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/essay-submissions/{submissionId}/grade - Teacher chấm thủ công (lần đầu hoặc cập nhật)
        [HttpPost("{submissionId}/grade")]
        public async Task<IActionResult> GradeManually(int submissionId, [FromBody] TeacherGradingDto dto)
        {
            var teacherId = User.GetUserId();
            var result = await _gradingService.GradeEssayAsync(submissionId, dto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/teacher/essay-submissions/{submissionId}/grade - Teacher cập nhật lại điểm đã chấm
        [HttpPut("{submissionId}/grade")]
        public async Task<IActionResult> UpdateGrade(int submissionId, [FromBody] TeacherGradingDto dto)
        {
            var teacherId = User.GetUserId();
            var result = await _gradingService.UpdateGradeAsync(submissionId, dto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/teacher/essay-submissions/essay/{essayId}/batch-grade-ai - Chấm hàng loạt bằng AI
        [HttpPost("essay/{essayId}/batch-grade-ai")]
        public async Task<IActionResult> BatchGradeByAi(int essayId)
        {
            var teacherId = User.GetUserId();
            var result = await _essaySubmissionService.BatchGradeByAiAsync(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/teacher/essay-submissions/essay/{essayId}/statistics - Thống kê essay
        [HttpGet("essay/{essayId}/statistics")]
        public async Task<IActionResult> GetEssayStatistics(int essayId)
        {
            var teacherId = User.GetUserId();
            var result = await _essaySubmissionService.GetEssayStatisticsAsync(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
