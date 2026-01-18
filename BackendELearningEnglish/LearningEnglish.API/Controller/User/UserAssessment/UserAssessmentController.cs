using LearningEnglish.Application.Interface;
using LearningEnglish.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningEnglish.API.Controller.User
{
    [Route("api/user/assessments")]
    [ApiController]
    [Authorize(Roles = "Student,SuperAdmin,ContentAdmin,FinanceAdmin")]
    public class UserAssessmentController : ControllerBase
    {
        private readonly IUserAssessmentService _assessmentService;
        private readonly ILogger<UserAssessmentController> _logger;

        public UserAssessmentController(IUserAssessmentService assessmentService, ILogger<UserAssessmentController> logger)
        {
            _assessmentService = assessmentService;
            _logger = logger;
        }

        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            var userId = User.GetUserId();
            var result = await _assessmentService.GetAssessmentsByModuleIdAsync(moduleId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            var userId = User.GetUserId();
            var result = await _assessmentService.GetAssessmentByIdAsync(assessmentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
