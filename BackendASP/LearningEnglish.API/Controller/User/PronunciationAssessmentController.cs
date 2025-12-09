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
        /// POST: api/PronunciationAssessment - Assess pronunciation in realtime
        /// Returns immediate results, updates only progress summary
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// GET: api/PronunciationAssessment/module/{moduleId} - Get flashcards with pronunciation progress
        /// </summary>
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetFlashCardsWithProgress(int moduleId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });

            var result = await _service.GetFlashCardsWithPronunciationProgressAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
