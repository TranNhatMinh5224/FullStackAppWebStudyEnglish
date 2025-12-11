using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.Shared
{
    [Route("api/shared/essay-submissions")]
    [ApiController]
    [Authorize]
    public class EssaySubmissionController : ControllerBase
    {
        private readonly IEssaySubmissionService _essaySubmissionService;

        public EssaySubmissionController(IEssaySubmissionService essaySubmissionService)
        {
            _essaySubmissionService = essaySubmissionService;
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

        // GET: api/shared/essay-submissions/{submissionId} - Get submission by ID (all roles with authorization)
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmission(int submissionId)
        {
            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/shared/essay-submissions/assessment/{assessmentId} - Get submissions by assessment (Admin/Teacher only)
        [HttpGet("assessment/{assessmentId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetSubmissionsByAssessment(int assessmentId)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essaySubmissionService.GetSubmissionsByAssessmentIdAsync(assessmentId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/shared/essay-submissions/user/{userId} - Get submissions by user (Admin/Teacher only)
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetSubmissionsByUser(int userId)
        {
            var result = await _essaySubmissionService.GetSubmissionsByUserIdAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
