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


        //controller lấy thông tin Submission theo ID

        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmission(int submissionId)
        {
            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

       
        //controller lấy danh sách Submission theo Assessment ID
       
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetSubmissionsByAssessment(int assessmentId)
        {
            // Lấy thông tin Teacher từ token (nếu không phải Admin)
            int? teacherId = null;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == "Teacher")
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    teacherId = parsedUserId;
                }
            }

            var result = await _essaySubmissionService.GetSubmissionsByAssessmentIdAsync(assessmentId, teacherId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        
        // Lấy danh sách Submission theo User ID 
        
        [HttpGet("user/{userId}")]
       
        public async Task<IActionResult> GetSubmissionsByUser(int userId)
        {
            var result = await _essaySubmissionService.GetSubmissionsByUserIdAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}