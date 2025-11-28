using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PronunciationAssessmentController : ControllerBase
    {
        private readonly IPronunciationAssessmentService _service;

        public PronunciationAssessmentController(IPronunciationAssessmentService service)
        {
            _service = service;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Create new pronunciation assessment with AI evaluation
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] CreatePronunciationAssessmentDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });

            var result = await _service.CreateAssessmentAsync(dto, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get assessment by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssessmentById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });

            var result = await _service.GetAssessmentByIdAsync(id, userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        /// <summary>
        /// Get all assessments for current user
        /// </summary>
        [HttpGet("my-assessments")]
        public async Task<IActionResult> GetMyAssessments()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });

            var result = await _service.GetUserAssessmentsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get assessments by FlashCard ID
        /// </summary>
        [HttpGet("flashcard/{flashCardId}")]
        public async Task<IActionResult> GetFlashCardAssessments(int flashCardId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });

            var result = await _service.GetFlashCardAssessmentsAsync(flashCardId, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Delete assessment
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessment(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });

            var result = await _service.DeleteAssessmentAsync(id, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Get user pronunciation statistics
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object> 
                { 
                    Success = false, 
                    Message = "User not authenticated" 
                });

            var result = await _service.GetUserStatisticsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// 🆕 Get progress analytics over time
        /// </summary>
        [HttpGet("progress")]
        public async Task<IActionResult> GetProgressAnalytics([FromQuery] int months = 3)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });

            var result = await _service.GetProgressAnalyticsAsync(userId, months);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // Đã xóa endpoint /comparative và method GetComparativeAnalytics theo yêu cầu
    }
}
