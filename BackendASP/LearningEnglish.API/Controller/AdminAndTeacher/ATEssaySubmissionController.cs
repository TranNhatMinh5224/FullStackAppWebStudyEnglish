using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    // quản lý essay submission cho cả Admin và Teacher
    [Route("api/admin-teacher/essay-submissions")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class ATEssaySubmissionController : ControllerBase
    {
        private readonly IEssaySubmissionService _essaySubmissionService;

        public ATEssaySubmissionController(IEssaySubmissionService essaySubmissionService)
        {
            _essaySubmissionService = essaySubmissionService;
        }

        // lấy danh sách submissions của một essay cụ thể với phân trang
        [HttpGet("essay/{essayId}/paged")]
        public async Task<IActionResult> GetSubmissionsByEssayPaged(
            int essayId,
            [FromQuery] PageRequest request)
        {
            var result = await _essaySubmissionService.GetSubmissionsByEssayIdPagedAsync(essayId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// lấy danh sách tất cả submissions của một essay cụ thể
        [HttpGet("essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionsByEssay(int essayId)
        {
            var result = await _essaySubmissionService.GetSubmissionsByEssayIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // lấy chi tiết một submission theo ID
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmissionDetail(int submissionId)
        {
            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // download file nộp bài của một submission
        [HttpGet("{submissionId}/download")]
        public async Task<IActionResult> DownloadSubmissionFile(int submissionId)
        {
            var result = await _essaySubmissionService.DownloadSubmissionFileAsync(submissionId);

            if (!result.Success)
            {
                return StatusCode(result.StatusCode, new { message = result.Message });
            }

            var (fileStream, fileName, contentType) = result.Data;

            // Trả về file để browser download
            return File(fileStream, contentType, fileName);
        }
    }
}
