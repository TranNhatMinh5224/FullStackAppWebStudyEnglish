using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/User/EssaySubmission")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class UserEssaySubmissionController : ControllerBase
    {
        private readonly IEssaySubmissionService _essaySubmissionService;

        public UserEssaySubmissionController(IEssaySubmissionService essaySubmissionService)
        {
            _essaySubmissionService = essaySubmissionService;
        }

        // controller nộp bài Essay(dành cho học sinh)
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitEssay([FromBody] CreateEssaySubmissionDto submissionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Không thể lấy thông tin người dùng từ token");
            }

            var result = await _essaySubmissionService.CreateSubmissionAsync(submissionDto, userId);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetSubmission), new { submissionId = result.Data?.SubmissionId }, result);
            }

            return BadRequest(result);
        }

       
        // Lấy thông tin submission

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

        
        // Lấy danh sách submission 
       
        [HttpGet("my-submissions")]
        public async Task<IActionResult> GetMySubmissions()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Không thể lấy thông tin người dùng từ token");
            }

            var result = await _essaySubmissionService.GetSubmissionsByUserIdAsync(userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        
        // Kiểm tra học sinh đã nộp bài cho Essay nào đó chưa
      
        [HttpGet("submission-status/essay/{essayId}")]
        public async Task<IActionResult> GetSubmissionStatus(int essayId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Không thể lấy thông tin người dùng từ token");
            }

            var result = await _essaySubmissionService.GetUserSubmissionForEssayAsync(userId, essayId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

       
        // Cập nhật submission của học sinh
     
        [HttpPut("update/{submissionId}")]
        public async Task<IActionResult> UpdateSubmission(int submissionId, [FromBody] UpdateEssaySubmissionDto updateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Không thể lấy thông tin người dùng từ token");
            }

            var result = await _essaySubmissionService.UpdateSubmissionAsync(submissionId, updateDto, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

   
        // Xóa submission của học sinh
        
        [HttpDelete("delete/{submissionId}")]
        public async Task<IActionResult> DeleteSubmission(int submissionId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("Không thể lấy thông tin người dùng từ token");
            }

            var result = await _essaySubmissionService.DeleteSubmissionAsync(submissionId, userId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}