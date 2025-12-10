using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/essay-submissions")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class UserEssaySubmissionController : ControllerBase
    {
        private readonly IEssaySubmissionService _essaySubmissionService;

        public UserEssaySubmissionController(IEssaySubmissionService essaySubmissionService)
        {
            _essaySubmissionService = essaySubmissionService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // POST: api/User/EssaySubmission/submit - Submit essay (for students)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitEssay([FromBody] CreateEssaySubmissionDto submissionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.CreateSubmissionAsync(submissionDto, userId);
            return result.Success
                ? CreatedAtAction("GetSubmission", "EssaySubmission", new { submissionId = result.Data?.SubmissionId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/my-submissions - Get all submissions for current user
        [HttpGet("my-submissions")]
        public async Task<IActionResult> GetMySubmissions()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.GetSubmissionsByUserIdAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/submission-status/essay/{essayId} - Check if student has submitted for an essay
        [HttpGet("submission-status/essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionStatus(int essayId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.GetUserSubmissionForEssayAsync(userId, essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/User/EssaySubmission/update/{submissionId} - Update student's submission
        [HttpPut("update/{submissionId}")]
        public async Task<IActionResult> UpdateSubmission(int submissionId, [FromBody] UpdateEssaySubmissionDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.UpdateSubmissionAsync(submissionId, updateDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/User/EssaySubmission/delete/{submissionId} - Delete student's submission
        [HttpDelete("delete/{submissionId}")]
        public async Task<IActionResult> DeleteSubmission(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.DeleteSubmissionAsync(submissionId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}