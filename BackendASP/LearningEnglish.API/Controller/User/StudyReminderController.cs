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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // GET: api/user/study-reminders - Get user's study reminders
        [HttpGet]
        public async Task<IActionResult> GetMyStudyReminders()
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _studyReminderService.GetUserStudyRemindersAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/study-reminders - Create study reminder
        [HttpPost]
        public async Task<IActionResult> CreateStudyReminder([FromBody] CreateStudyReminderDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            request.UserId = userId;
            var result = await _studyReminderService.CreateStudyReminderAsync(request);
            return result.Success ? Created("", result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/study-reminders/{reminderId} - Update study reminder
        [HttpPut("{reminderId}")]
        public async Task<IActionResult> UpdateStudyReminder(int reminderId, [FromBody] CreateStudyReminderDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _studyReminderService.UpdateStudyReminderAsync(reminderId, request, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/user/study-reminders/{reminderId} - Delete study reminder
        [HttpDelete("{reminderId}")]
        public async Task<IActionResult> DeleteStudyReminder(int reminderId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _studyReminderService.DeleteStudyReminderAsync(reminderId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // PUT: api/user/study-reminders/{reminderId}/toggle - Toggle study reminder on/off
        [HttpPut("{reminderId}/toggle")]
        public async Task<IActionResult> ToggleStudyReminder(int reminderId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _studyReminderService.ToggleStudyReminderAsync(reminderId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/study-reminders/{reminderId}/send-now - Send reminder immediately
        [HttpPost("{reminderId}/send-now")]
        public async Task<IActionResult> SendReminderNow(int reminderId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _studyReminderService.SendReminderNowAsync(reminderId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}