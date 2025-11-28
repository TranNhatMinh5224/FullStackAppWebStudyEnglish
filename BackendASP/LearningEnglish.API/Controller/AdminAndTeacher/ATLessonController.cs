using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/lesson/")]
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
        // Controller thêm Lesson mới Admin 

        [HttpPost("admin/add")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddLesson(AdminCreateLessonDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _lessonService.AdminAddLesson(dto);

                if (!response.Success) 
                {
                    return BadRequest(response);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson for admin");
                return StatusCode(500, "Internal server error");
            }
        }
        // Controller thêm Lesson mới Teacher

        [HttpPost("teacher/add")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> AddLessonForTeacher(TeacherCreateLessonDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var response = await _lessonService.TeacherAddLesson(dto);
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding lesson for teacher");
                return StatusCode(500, "Internal server error");
            }
        }
        // Controller xóa Lesson

        [HttpDelete("delete/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            try
            {

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user credentials" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { Message = "User role not found" });
                }

                // Call service method with authorization
                var response = await _lessonService.DeleteLessonWithAuthorizationAsync(lessonId, userId, userRole);

                if (!response.Success)
                {
                    var message = response.Message ?? "Operation failed";
                    if (message.Contains("not found"))
                        return NotFound(new { Message = message });
                    if (message.Contains("permission") || message.Contains("own courses"))
                        return StatusCode(403, new { Message = message });
                    return BadRequest(new { Message = message });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lesson {LessonId} by user {UserId}", lessonId, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
        // Controller cập nhật Lesson

        [HttpPut("update/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] UpdateLessonDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Extract user information from JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user credentials" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { Message = "User role not found" });
                }

                // Call service method with authorization
                var response = await _lessonService.UpdateLessonWithAuthorizationAsync(lessonId, dto, userId, userRole);

                if (!response.Success)
                {
                    var message = response.Message ?? "Operation failed";
                    if (message.Contains("not found"))
                        return NotFound(new { Message = message });
                    if (message.Contains("permission") || message.Contains("own courses"))
                        return StatusCode(403, new { Message = message });
                    return BadRequest(new { Message = message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating lesson {LessonId}", lessonId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
        // Controller lấy Lesson theo ID

        [HttpGet("get/{lessonId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<IActionResult> GetLessonById(int lessonId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User role not found" });
                }

                var response = await _lessonService.GetLessonById(lessonId, userId, userRole);
                if (!response.Success)
                {
                    if (response.StatusCode == 404)
                    {
                        return NotFound(new { message = response.Message });
                    }
                    if (response.StatusCode == 403)
                    {
                        return StatusCode(403, new { message = response.Message });
                    }
                    return BadRequest(new { message = response.Message });
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lesson {LessonId}", lessonId);
                return StatusCode(500, "Internal server error");
            }
        }
        // Controller lấy danh sách Lesson theo Course ID

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetListLessonByCourseId(int courseId)
        {
            try
            {

           
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user credentials" });
                }

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(userRole))
                {
                    return Unauthorized(new { message = "User role not found" });
                }

             
                var response = await _lessonService.GetListLessonByCourseId(courseId, userId, userRole);

                if (!response.Success)
                {
                    if (response.StatusCode == 404)
                    {
                        return NotFound(new { message = response.Message });
                    }
                    if (response.StatusCode == 403)
                    {
                        return StatusCode(403, new { message = response.Message });
                    }
                    return BadRequest(new { message = response.Message });
                }

                return Ok(response);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lessons for course {CourseId}", courseId);
                return StatusCode(500, "Internal server error");
            }
        }

    }
}
