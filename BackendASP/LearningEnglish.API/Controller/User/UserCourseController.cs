using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using System.Security.Claims;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/courses")]
    public class UserCourseController : ControllerBase
    {
        private readonly IUserCourseService _userCourseService;
        private readonly ILogger<UserCourseController> _logger;

        public UserCourseController(IUserCourseService userCourseService, ILogger<UserCourseController> logger)
        {
            _userCourseService = userCourseService;
            _logger = logger;
        }

        
        //Lấy danh sách khóa học hệ thống (không cần đăng nhập)
        
        [HttpGet("system-courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemCourses()
        {
            try
            {
                // Lấy UserId nếu user đã đăng nhập (optional)
                int? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                var result = await _userCourseService.GetSystemCoursesAsync(userId);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSystemCourses endpoint");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
