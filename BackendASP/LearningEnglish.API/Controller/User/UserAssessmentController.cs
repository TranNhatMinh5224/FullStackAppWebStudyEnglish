using LearningEnglish.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("module/{moduleId}")]
        public async Task<IActionResult> GetAssessmentsByModuleId(int moduleId)
        {
            var result = await _assessmentService.GetAssessmentsByModuleId(moduleId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        [HttpGet("{assessmentId}")]
        public async Task<IActionResult> GetAssessmentById(int assessmentId)
        {
            var result = await _assessmentService.GetAssessmentById(assessmentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
