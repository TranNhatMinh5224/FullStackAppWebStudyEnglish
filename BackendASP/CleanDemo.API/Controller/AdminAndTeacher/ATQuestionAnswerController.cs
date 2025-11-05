using System.Security.Claims;
using System.Threading.Tasks;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CleanDemo.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/question-answer")]
    [Authorize]
    public class ATQuestionAnswerController : ControllerBase
    {
        private readonly IQuestionAnswerService _service;
        private readonly ILogger<ATQuestionAnswerController> _logger;

        public ATQuestionAnswerController(IQuestionAnswerService service, ILogger<ATQuestionAnswerController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> CreateQuestion([FromBody] CreateQuestionDto dto)
        {
            try
            {
                int? actorId = null;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out var uid)) actorId = uid;

                var result = await _service.CreateQuestionAsync(dto, actorId);
                return StatusCode(result.StatusCode, result);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in CreateQuestion");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
