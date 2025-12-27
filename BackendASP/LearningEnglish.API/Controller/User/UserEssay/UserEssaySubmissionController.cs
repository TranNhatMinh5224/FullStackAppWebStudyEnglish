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
        private readonly IUserEssaySubmissionService _essaySubmissionService;

        public UserEssaySubmissionController(IUserEssaySubmissionService essaySubmissionService)
        {
            _essaySubmissionService = essaySubmissionService;
        }

        // POST: api/User/EssaySubmission/submit - nop bai essay
       
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitEssay([FromBody] CreateEssaySubmissionDto submissionDto)
        {
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.CreateSubmissionAsync(submissionDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/{submissionId} - lấy bài nộp theo ID
      
        public async Task<IActionResult> GetSubmission(int submissionId)
        {
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.GetMySubmissionByIdAsync(submissionId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/EssaySubmission/submission-status/essay/{essayId} - kiem tra trang thai nop bai theo essay ID
       
        [HttpGet("submission-status/essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionStatus(int essayId)
        {
            var userId = User.GetUserId();

            var result = await _essaySubmissionService.GetMySubmissionForEssayAsync(userId, essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/User/EssaySubmission/update/{submissionId} - sua bai nop essay
        
        [HttpPut("update/{submissionId}")]
        public async Task<IActionResult> UpdateSubmission(int submissionId, [FromBody] UpdateEssaySubmissionDto updateDto)
        {
            var userId = User.GetUserId();
            
            var result = await _essaySubmissionService.UpdateSubmissionAsync(submissionId, updateDto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/User/EssaySubmission/delete/{submissionId} - xoa bai nop essay
       
        [HttpDelete("delete/{submissionId}")]
        public async Task<IActionResult> DeleteSubmission(int submissionId)
        {
            var userId = User.GetUserId();
         
            var result = await _essaySubmissionService.DeleteSubmissionAsync(submissionId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}