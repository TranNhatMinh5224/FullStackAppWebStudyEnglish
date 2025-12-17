using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    // GET: api/user/essays - quản lý essay cho user role Student
    [Route("api/user/essays")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class UserEssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public UserEssayController(IEssayService essayService)
        {
            _essayService = essayService;
        }

        // GET: api/user/essays/{essayId} - lấy essay theo ID (chỉ đọc)
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/essays/assessment/{assessmentId} - lấy danh sách essay theo assessment ID (chỉ đọc)
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
