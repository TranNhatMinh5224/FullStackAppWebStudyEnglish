using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{

    // quản lý essay cho cả Admin và Teacher

    [Route("api/admin-teacher/essays")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class ATEssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public ATEssayController(IEssayService essayService)
        {
            _essayService = essayService;
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


        // POST: tạo mới essay
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ tạo essay cho assessments của own courses
        [HttpPost]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> CreateEssay([FromBody] CreateEssayDto createDto)
        {
            var userRole = User.GetPrimaryRole();
            int? teacherId = userRole == "Teacher" ? User.GetUserId() : null;

            var result = await _essayService.CreateEssayAsync(createDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetEssay), new { essayId = result.Data?.EssayId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: sửa essay
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ sửa essay của own courses (RLS check)
        [HttpPut("{essayId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateEssay(int essayId, [FromBody] UpdateEssayDto updateDto)
        {
            var userRole = User.GetPrimaryRole();
            int? teacherId = userRole == "Teacher" ? User.GetUserId() : null;

            var result = await _essayService.UpdateEssayAsync(essayId, updateDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: xóa essay
        // Admin: Cần permission Admin.Content.Manage
        // Teacher: Chỉ xóa essay của own courses (RLS check)
        [HttpDelete("{essayId}")]
        [RequirePermission("Admin.Content.Manage")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> DeleteEssay(int essayId)
        {
            var userRole = User.GetPrimaryRole();
            int? teacherId = userRole == "Teacher" ? User.GetUserId() : null;

            var result = await _essayService.DeleteEssayAsync(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
