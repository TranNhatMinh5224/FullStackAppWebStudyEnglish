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

        // POST: api/PronunciationAssessment - Create new pronunciation assessment with AI evaluation
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/{id} - Get assessment by ID
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/my-assessments - Get all assessments for current user
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/flashcard/{flashCardId} - Get assessments by FlashCard ID
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/PronunciationAssessment/{id} - Delete assessment
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/statistics - Get user pronunciation statistics
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/progress - Get progress analytics over time
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
