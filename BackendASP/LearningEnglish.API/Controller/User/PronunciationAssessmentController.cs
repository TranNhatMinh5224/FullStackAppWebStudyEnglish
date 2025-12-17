using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // POST: api/PronunciationAssessment - tạo mới bài đánh giá phát âm
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

        // GET: api/PronunciationAssessment/module/{moduleId} - lấy danh sách flashcard kèm tiến độ phát âm theo module ID
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

        // GET: api/PronunciationAssessment/module/{moduleId}/card/{cardIndex} - lấy flashcard kèm phát âm theo chỉ số cho luyện tập
        [HttpGet("module/{moduleId}/card/{cardIndex}")]
        public async Task<IActionResult> GetFlashCardByIndexForPractice(int moduleId, int cardIndex)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new ServiceResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });

            var result = await _service.GetFlashCardWithPronunciationByIndexAsync(moduleId, cardIndex, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
