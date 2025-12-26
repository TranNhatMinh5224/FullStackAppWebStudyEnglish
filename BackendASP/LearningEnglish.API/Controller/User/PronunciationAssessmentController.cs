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

        // POST: api/PronunciationAssessment - tạo mới bài đánh giá phát âm
        // RLS: pronunciationprogresses_policy_user_all_own
        [HttpPost]
        public async Task<IActionResult> CreateAssessment([FromBody] CreatePronunciationAssessmentDto dto)
        {
            var userId = User.GetUserId();

            var result = await _service.CreateAssessmentAsync(dto, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/module/{moduleId} - lấy danh sách flashcard kèm tiến độ phát âm theo module ID
        // RLS: pronunciationprogresses_policy_user_all_own (chỉ xem progress của chính mình)
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetFlashCardsWithProgress(int moduleId)
        {
            // [Authorize] đảm bảo userId luôn có
            // RLS sẽ filter pronunciation progresses theo userId
            var userId = User.GetUserId();

            var result = await _service.GetFlashCardsWithPronunciationProgressAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/PronunciationAssessment/module/{moduleId}/summary - lấy tổng hợp kết quả/thống kê module
        // RLS: pronunciationprogresses_policy_user_all_own (chỉ xem progress của chính mình)
        [HttpGet("module/{moduleId}/summary")]
        public async Task<IActionResult> GetModulePronunciationSummary(int moduleId)
        {
            // [Authorize] đảm bảo userId luôn có
            // RLS sẽ filter pronunciation progresses theo userId
            var userId = User.GetUserId();

            var result = await _service.GetModulePronunciationSummaryAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
