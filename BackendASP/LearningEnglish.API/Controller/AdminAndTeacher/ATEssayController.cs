using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{

    // quản lý essay cho cả Admin và Teacher

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

        // lấy essay theo ID
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // lấy danh sách essay theo assessment ID
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }


        // tạo mới essay

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

        //sửa essay
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

        // xoa essay
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
