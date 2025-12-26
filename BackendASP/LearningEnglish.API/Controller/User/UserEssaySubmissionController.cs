using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        // POST: api/User/EssaySubmission/submit - nop bai essay
        // RLS: essaysubmissions_policy_student_all_own
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitEssay([FromBody] CreateEssaySubmissionDto submissionDto)
        {
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.CreateSubmissionAsync(submissionDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/{submissionId} - lấy bài nộp theo ID
        // RLS: essaysubmissions_policy_student_all_own (chỉ xem submissions của chính mình)
        [HttpGet("{submissionId}")]
        public async Task<IActionResult> GetSubmission(int submissionId)
        {
            // RLS sẽ filter essay submissions theo userId
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.GetSubmissionByIdAsync(submissionId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/submission-status/essay/{essayId} - kiem tra trang thai nop bai theo essay ID
        // RLS: essaysubmissions_policy_student_all_own (chỉ xem submissions của chính mình)
        [HttpGet("submission-status/essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionStatus(int essayId)
        {
            // RLS sẽ filter essay submissions theo userId
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.GetUserSubmissionForEssayAsync(userId, essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/User/EssaySubmission/update/{submissionId} - sua bai nop essay
        // RLS: essaysubmissions_policy_student_all_own (chỉ update submissions của chính mình)
        // FluentValidation: UpdateEssaySubmissionDtoValidator sẽ tự động validate
        [HttpPut("update/{submissionId}")]
        public async Task<IActionResult> UpdateSubmission(int submissionId, [FromBody] UpdateEssaySubmissionDto updateDto)
        {
            // RLS sẽ filter essay submissions theo userId
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.UpdateSubmissionAsync(submissionId, updateDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/User/EssaySubmission/delete/{submissionId} - xoa bai nop essay
        // RLS: essaysubmissions_policy_student_all_own (chỉ xóa submissions của chính mình)
        [HttpDelete("delete/{submissionId}")]
        public async Task<IActionResult> DeleteSubmission(int submissionId)
        {
            // RLS sẽ filter essay submissions theo userId
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.DeleteSubmissionAsync(submissionId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}