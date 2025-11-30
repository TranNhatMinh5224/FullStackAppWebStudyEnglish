using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/User/Essay")]
    [ApiController]
    [Authorize(Roles = "Student")]
    public class UserEssayController : ControllerBase
    {
        private readonly IEssayService _essayService;

        public UserEssayController(IEssayService essayService)
        {
            _essayService = essayService;
        }

        // GET: api/User/Essay/{essayId} - Get essay by ID (for students)
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/User/Essay/assessment/{assessmentId} - Get essays by assessment ID (for students)
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}