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
       
        [HttpGet("system-courses")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemCourses()
        {
            
            int? userId = userIdValue > 0 ? userIdValue : null;

            var result = await _userCourseService.GetSystemCoursesAsync(userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Guest/User lấy chi tiết khóa học theo ID
        [HttpGet("{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseById(int courseId)
        {
            
            var userIdValue = User.GetUserIdSafe();
            

            var result = await _userCourseService.GetCourseByIdAsync(courseId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
        
        // endpoint Guest/User tìm kiếm khóa học
       
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchCourses([FromQuery] string keyword)
        {
          
            var result = await _userCourseService.SearchCoursesAsync(keyword);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
