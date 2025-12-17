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

        // POST: api/User/EssaySubmission/submit - nop bai essay
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitEssay([FromBody] CreateEssaySubmissionDto submissionDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.CreateSubmissionAsync(submissionDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/{submissionId} - lấy bài nộp theo ID
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmission(int submissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/submission-status/essay/{essayId} - kiem tra trang thai nop bai theo essay ID
        [HttpGet("submission-status/essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionStatus(int essayId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return BadRequest("Không thể lấy thông tin người dùng từ token");

            var result = await _essaySubmissionService.GetUserSubmissionForEssayAsync(userId, essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/User/EssaySubmission/update/{submissionId} - sua bai nop essay
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

        // DELETE: api/User/EssaySubmission/delete/{submissionId} - xoa bai nop essay
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