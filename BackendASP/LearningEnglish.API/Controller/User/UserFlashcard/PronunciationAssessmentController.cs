using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{

    [ApiController]
    [Route("api/user/pronunciation-assessments")]
    [Authorize]
    public class PronunciationAssessmentController : ControllerBase
    {
        private readonly IPronunciationAssessmentService _service;

        public PronunciationAssessmentController(IPronunciationAssessmentService service)
        {
            _service = service;
        }

        // POST: api/PronunciationAssessment 
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] CreatePronunciationAssessmentDto dto)
        {
            var userId = User.GetUserId();

            var result = await _service.CreateAssessmentAsync(dto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetFlashCardsWithProgress(int moduleId)
        {
            
            var userId = User.GetUserId();

            var result = await _service.GetFlashCardsWithPronunciationProgressAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        
        [HttpGet("module/{moduleId}/summary")]
        public async Task<IActionResult> GetModulePronunciationSummary(int moduleId)
        {
            
            var userId = User.GetUserId();

            var result = await _service.GetModulePronunciationSummaryAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
