using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    /// <summary>
    /// Controller quản lý Essay Submissions cho Admin và Teacher
    /// Admin: Xem tất cả submissions trong hệ thống
    /// Teacher: Chỉ xem submissions từ courses mình dạy (filtered by RLS)
    /// </summary>
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

        /// <summary>
        /// Lấy danh sách submissions của một essay cụ thể (CÓ PHÂN TRANG)
        /// Trả về thông tin cơ bản: UserId, UserName, SubmittedAt, Status
        /// - Teacher: Chỉ thấy submissions từ essays trong courses mình dạy (RLS tự động filter)
        /// - Admin: Xem tất cả submissions
        /// </summary>
        /// <param name="essayId">ID của essay cần xem submissions</param>
        /// <param name="pageNumber">Số trang (mặc định: 1)</param>
        /// <param name="pageSize">Số items trên mỗi trang (mặc định: 10)</param>
        /// <param name="searchTerm">Tìm kiếm theo tên học sinh (optional)</param>
        /// <returns>Danh sách submissions với phân trang (basic info)</returns>
        [HttpGet("essay/{essayId}/paged")]
        public async Task<IActionResult> GetSubmissionsByEssayPaged(
            int essayId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            var request = new PageRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm
            };

            var result = await _essaySubmissionService.GetSubmissionsByEssayIdPagedAsync(essayId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách submissions của một essay cụ thể (KHÔNG PHÂN TRANG)
        /// Trả về thông tin cơ bản: UserId, UserName, SubmittedAt, Status
        /// - Teacher: Chỉ thấy submissions từ essays trong courses mình dạy (RLS tự động filter)
        /// - Admin: Xem tất cả submissions
        /// </summary>
        /// <param name="essayId">ID của essay cần xem submissions</param>
        /// <returns>Danh sách TẤT CẢ submissions (basic info)</returns>
        [HttpGet("essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionsByEssay(int essayId)
        {
            var result = await _essaySubmissionService.GetSubmissionsByEssayIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xem chi tiết một submission cụ thể
        /// - Teacher: Chỉ thấy submissions từ courses mình dạy (RLS tự động filter)
        /// - Admin: Xem tất cả submissions
        /// </summary>
        /// <param name="submissionId">ID của submission</param>
        /// <returns>Chi tiết submission</returns>
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmissionDetail(int submissionId)
        {
            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Download file đính kèm của submission
        /// - Teacher: Chỉ download được files từ courses mình dạy (RLS tự động filter)
        /// - Admin: Download tất cả files
        /// </summary>
        /// <param name="submissionId">ID của submission</param>
        /// <returns>File stream để download</returns>
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
