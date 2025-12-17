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

        // GET: api/user/courses/system-courses - lấy danh sách tất cả khóa học hệ thống với trạng thái đăng ký của người dùng (nếu đã đăng nhập)
        [HttpGet("system-courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemCourses()
        {
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
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // Lấy chi tiết khóa học theo ID với trạng thái đăng ký
        [HttpGet("{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            int? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int parsedUserId))
                {
                    userId = parsedUserId;
                }
            }

            var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
        // Tìm kiếm khóa học theo từ khóa
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourses([FromQuery] string keyword)
        {
            var result = await _userCourseService.SearchCoursesAsync(keyword);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
