using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOS;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/User/QuizAttempt")]
    [Authorize(Roles = "Student")]
    public class UserQuizAttemptController : ControllerBase
    {
        private readonly IQuizAttemptService _attemptService;

        public UserQuizAttemptController(IQuizAttemptService attemptService)
        {
            _attemptService = attemptService;
        }

        [HttpPost("Start")]
        public async Task<IActionResult> StartAttempt([FromBody] StartQuizAttemptRequestDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _attemptService.StartAttemptAsync(userId, request);

            if (result.Success)
                return Ok(result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{attemptId}/Answer")]
        public async Task<IActionResult> UpdateAnswer(int attemptId, [FromBody] UpdateAnswerDto answerDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _attemptService.UpdateAnswerAsync(userId, attemptId, answerDto);

            if (result.Success)
                return Ok(result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{attemptId}")]
        public async Task<IActionResult> GetAttempt(int attemptId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _attemptService.GetAttemptAsync(userId, attemptId);

            if (result.Success)
                return Ok(result);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{attemptId}/Finish")]
        public async Task<IActionResult> FinishAttempt(int attemptId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _attemptService.FinishAttemptAsync(userId, attemptId);

            if (result.Success)
                return Ok(result);

            return StatusCode(result.StatusCode, result);
        }
    }
}
