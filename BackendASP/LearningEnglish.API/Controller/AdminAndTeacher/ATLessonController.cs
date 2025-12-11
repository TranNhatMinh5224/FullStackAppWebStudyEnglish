using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/lessons")]
    [Authorize]
    public class ATLessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;
        private readonly ILogger<ATLessonController> _logger;

        public ATLessonController(ILessonService lessonService, ILogger<ATLessonController> logger)
        {
            _lessonService = lessonService;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("Invalid user credentials");
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(userRole))
            {
                throw new UnauthorizedAccessException("User role not found");
            }
            return userRole;
        }

        // POST: api/lesson/admin/add - Admin creates a new lesson
        [HttpPost("admin/add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLesson(AdminCreateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _lessonService.AdminAddLesson(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: api/lesson/teacher/add - Teacher creates a new lesson for their course
        [HttpPost("teacher/add")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddLessonForTeacher(TeacherCreateLessonDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _lessonService.TeacherAddLesson(dto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/lesson/delete/{lessonId} - Delete lesson (Admin: any, Teacher: own only)
        [HttpDelete("delete/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lessonService.DeleteLessonWithAuthorizationAsync(lessonId, userId, userRole);
            return result.Success ? NoContent() : StatusCode(result.StatusCode, result);
        }

        // PUT: api/lesson/update/{lessonId} - Update lesson (Admin: any, Teacher: own only)
        [HttpPut("update/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lessonService.UpdateLessonWithAuthorizationAsync(lessonId, dto, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/lesson/get/{lessonId} - Get lesson details by ID
        [HttpGet("get/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lessonService.GetLessonById(lessonId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: api/lesson/course/{courseId} - Get all lessons for a course
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();
            var result = await _lessonService.GetListLessonByCourseId(courseId, userId, userRole);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
