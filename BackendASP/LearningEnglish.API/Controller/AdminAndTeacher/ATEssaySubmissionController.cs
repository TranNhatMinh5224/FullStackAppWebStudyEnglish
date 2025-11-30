using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [Route("api/EssaySubmission")]
    [ApiController]
    [Authorize(Roles = "Admin, Teacher")]
    public class ATEssaySubmissionController : ControllerBase
    {
        private readonly IEssaySubmissionService _essaySubmissionService;

        public ATEssaySubmissionController(IEssaySubmissionService essaySubmissionService)
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

        // GET: api/EssaySubmission/{submissionId} - Get submission by ID
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmission(int submissionId)
        {
            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/EssaySubmission/assessment/{assessmentId} - Get submissions by assessment ID (Admin: all, Teacher: own)
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetSubmissionsByAssessment(int assessmentId)
        {
            var userRole = GetCurrentUserRole();
            int? teacherId = userRole == "Teacher" ? GetCurrentUserId() : null;

            var result = await _essaySubmissionService.GetSubmissionsByAssessmentIdAsync(assessmentId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/EssaySubmission/user/{userId} - Get submissions by user ID
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetSubmissionsByUser(int userId)
        {
            var result = await _essaySubmissionService.GetSubmissionsByUserIdAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}