using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.API.Extensions;

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

        // endpoint Guest/User lấy danh sách khóa học hệ thống
        // RLS: courses_policy_guest_select (Guest có thể xem system courses)
        [HttpGet("system-courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemCourses()
        {
            // [AllowAnonymous] nên userId có thể null (Guest) hoặc có (authenticated user)
            // RLS sẽ filter courses theo role (Guest/Student/Teacher/Admin)
            var userIdValue = User.GetUserIdSafe();
            int? userId = userIdValue > 0 ? userIdValue : null;

            var result = await _userCourseService.GetSystemCoursesAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Guest/User xem chi tiết khóa học
        // RLS: courses_policy_guest_select (Guest có thể xem system courses) hoặc courses_policy_student_select_enrolled (Student xem enrolled courses)
        [HttpGet("{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            // [AllowAnonymous] nên userId có thể null (Guest) hoặc có (authenticated user)
            // RLS sẽ filter courses theo role (Guest/Student/Teacher/Admin)
            var userIdValue = User.GetUserIdSafe();
            int? userId = userIdValue > 0 ? userIdValue : null;

            var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
        
        // endpoint Guest/User tìm kiếm khóa học
        // RLS: courses_policy_guest_select (Guest có thể xem system courses)
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourses([FromQuery] string keyword)
        {
            // RLS sẽ filter courses theo role (Guest/Student/Teacher/Admin)
            var result = await _userCourseService.SearchCoursesAsync(keyword);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
