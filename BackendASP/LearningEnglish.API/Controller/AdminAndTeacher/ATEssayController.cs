using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    /// <summary>
    /// Controller quản lý Essays cho Admin và Teacher
    /// Admin: CRUD tất cả essays trong hệ thống
    /// Teacher: CRUD chỉ essays thuộc courses mình dạy (filtered by RLS hoặc teacherId check)
    /// </summary>
    [Route("api/admin-teacher/essays")]
    [ApiController]
    [Authorize(Roles = "Admin,Teacher")]
    public class ATEssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public ATEssayController(IEssayService essayService)
        {
            _essayService = essayService;
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

        /// <summary>
        /// Xem chi tiết một essay
        /// - Teacher: Chỉ xem essays từ courses mình dạy
        /// - Admin: Xem tất cả
        /// </summary>
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Lấy danh sách essays theo assessment ID
        /// - Teacher: Chỉ thấy essays từ courses mình dạy
        /// - Admin: Xem tất cả
        /// </summary>
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Tạo essay mới
        /// - Teacher: Chỉ tạo essays trong courses mình dạy (kiểm tra teacherId)
        /// - Admin: Tạo essays trong bất kỳ course nào
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateEssay([FromBody] CreateEssayDto createDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essayService.CreateEssayAsync(createDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetEssay), new { essayId = result.Data?.EssayId }, result)
                : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Cập nhật essay
        /// - Teacher: Chỉ cập nhật essays trong courses mình dạy (kiểm tra teacherId)
        /// - Admin: Cập nhật bất kỳ essay nào
        /// </summary>
        [HttpPut("{essayId}")]
        public async Task<IActionResult> UpdateEssay(int essayId, [FromBody] UpdateEssayDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essayService.UpdateEssayAsync(essayId, updateDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Xóa essay
        /// - Teacher: Chỉ xóa essays trong courses mình dạy (kiểm tra teacherId)
        /// - Admin: Xóa bất kỳ essay nào
        /// </summary>
        [HttpDelete("{essayId}")]
        public async Task<IActionResult> DeleteEssay(int essayId)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essayService.DeleteEssayAsync(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
