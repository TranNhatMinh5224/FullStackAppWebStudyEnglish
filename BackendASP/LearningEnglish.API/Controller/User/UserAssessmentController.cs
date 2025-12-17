using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/assessments")]
    [ApiController]
    [Authorize]
    public class UserAssessmentController : ControllerBase
    {
        private readonly IAssessmentService _assessmentService;
        private readonly ILogger<UserAssessmentController> _logger;

        public UserAssessmentController(IAssessmentService assessmentService, ILogger<UserAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: api/user/assessment/module/{moduleId} - get danh sách assessment theo module ID
        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/assessment/{assessmentId} - lay chi tiết assessment theo ID
        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            var result = await _assessmentService.GetAssessmentById(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
