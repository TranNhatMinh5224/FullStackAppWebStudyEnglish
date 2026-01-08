using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.Admin
{
    [Route("api/admin/essay-submissions")]
    [ApiController]
    [Authorize]
    [RequirePermission("Admin.Content.Manage")]
    public class AdminEssaySubmissionController : ControllerBase
    {
        private readonly IAdminEssaySubmissionService _essaySubmissionService;
        private readonly IAdminEssayGradingService _gradingService;

        public AdminEssaySubmissionController(
            IAdminEssaySubmissionService essaySubmissionService,
            IAdminEssayGradingService gradingService)
        {
            _essaySubmissionService = essaySubmissionService;
            _gradingService = gradingService;
        }

        // GET: api/admin/essay-submissions/essay/{essayId} - Lấy danh sách submissions với phân trang
        [HttpGet("essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionsByEssay(
            int essayId,
            [FromQuery] PageRequest request)
        {
            var result = await _essaySubmissionService.GetSubmissionsByEssayIdPagedAsync(essayId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/essay-submissions/{submissionId}
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmissionDetail(int submissionId)
        {
            var result = await _essaySubmissionService.GetSubmissionDetailAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/admin/essay-submissions/{submissionId}/download
        [HttpGet("{submissionId}/download")]
        public async Task<IActionResult> DownloadSubmissionFile(int submissionId)
        {
            var result = await _essaySubmissionService.DownloadSubmissionFileAsync(submissionId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            var (fileStream, fileName, contentType) = result.Data;
            return File(fileStream, contentType, fileName);
        }

        // DELETE: api/admin/essay-submissions/{submissionId}
        [HttpDelete("{submissionId}")]
        public async Task<IActionResult> DeleteSubmission(int submissionId)
        {
            var result = await _essaySubmissionService.DeleteSubmissionAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/essay-submissions/{submissionId}/grade-ai
        [HttpPost("{submissionId}/grade-ai")]
        public async Task<IActionResult> GradeWithAI(int submissionId)
        {
            var result = await _gradingService.GradeEssayWithAIAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/essay-submissions/{submissionId}/grade
        [HttpPost("{submissionId}/grade")]
        public async Task<IActionResult> GradeManually(int submissionId, [FromBody] TeacherGradingDto dto)
        {
            var result = await _gradingService.GradeByAdminAsync(submissionId, dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/admin/essay-submissions/essay/{essayId}/batch-grade-ai - Admin chấm hàng loạt bằng AI
        [HttpPost("essay/{essayId}/batch-grade-ai")]
        public async Task<IActionResult> BatchGradeByAi(int essayId)
        {
            var result = await _gradingService.BatchGradeByAiAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
