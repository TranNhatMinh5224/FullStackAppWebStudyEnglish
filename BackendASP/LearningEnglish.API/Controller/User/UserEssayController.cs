using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        
        // Lấy thông tin Essay theo ID (dành cho học sinh)
       
        [HttpGet("{essayId}")]
        public async Task<IActionResult> GetEssay(int essayId)
        {
            var result = await _essayService.GetEssayByIdAsync(essayId);

            if (result.Success)
            {
                return Ok(result);
            }

            return NotFound(result);
        }

        // Lấy danh sách Essay theo Assessment ID (dành cho học sinh)
        [HttpGet("assessment/{assessmentId}")]
        public async Task<IActionResult> GetEssaysByAssessment(int assessmentId)
        {
            var result = await _essayService.GetEssaysByAssessmentIdAsync(assessmentId);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
    }
}