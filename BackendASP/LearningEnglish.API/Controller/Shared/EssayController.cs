using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.Shared
{
    [Route("api/shared/essays")]
    [ApiController]
    [Authorize]
    public class EssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public EssayController(IEssayService essayService)
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

        // GET: api/shared/essays/{essayId} - Get essay by ID (all roles)
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/shared/essays/assessment/{assessmentId} - Get essays by assessment ID (all roles)
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/shared/essays - Create new essay (Admin/Teacher only)
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
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

        // PUT: api/shared/essays/{essayId} - Update essay (Admin: any, Teacher: own only)
        [HttpPut("{essayId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateEssay(int essayId, [FromBody] UpdateEssayDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essayService.UpdateEssayAsync(essayId, updateDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/shared/essays/{essayId} - Delete essay (Admin: any, Teacher: own only)
        [HttpDelete("{essayId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteEssay(int essayId)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essayService.DeleteEssayAsync(essayId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
