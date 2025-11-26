using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/study-reminders")]
    [Authorize]
    public class StudyReminderController : ControllerBase
    {
        private readonly IStudyReminderService _studyReminderService;

        public StudyReminderController(IStudyReminderService studyReminderService)
        {
            _studyReminderService = studyReminderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyStudyReminders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _studyReminderService.GetUserStudyRemindersAsync(userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudyReminder([FromBody] CreateStudyReminderDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            request.UserId = userId;

            var result = await _studyReminderService.CreateStudyReminderAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Created("", result);
        }

        [HttpPut("{reminderId}")]
        public async Task<IActionResult> UpdateStudyReminder(int reminderId, [FromBody] CreateStudyReminderDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _studyReminderService.UpdateStudyReminderAsync(reminderId, request, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{reminderId}")]
        public async Task<IActionResult> DeleteStudyReminder(int reminderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _studyReminderService.DeleteStudyReminderAsync(reminderId, int.Parse(userIdClaim));
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("{reminderId}/toggle")]
        public async Task<IActionResult> ToggleStudyReminder(int reminderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _studyReminderService.ToggleStudyReminderAsync(reminderId, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPost("{reminderId}/send-now")]
        public async Task<IActionResult> SendReminderNow(int reminderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user credentials" });
            }

            var result = await _studyReminderService.SendReminderNowAsync(reminderId, userId);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}