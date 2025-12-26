using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;

namespace LearningEnglish.API.Controller.User
{
    [ApiController]
    [Route("api/user/enrollments")]
    [Authorize(Roles = "Student")]
    public class EnrollCourseController : ControllerBase
    {
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly IEnrollmentQueryService _enrollmentQueryService;

        public EnrollCourseController(
            IUserEnrollmentService userEnrollmentService,
            IEnrollmentQueryService enrollmentQueryService)
        {
            _userEnrollmentService = userEnrollmentService;
            _enrollmentQueryService = enrollmentQueryService;
        }

        // endpoint Student đăng ký khóa học
        [HttpPost("course")]
        public async Task<IActionResult> EnrollInCourse([FromBody] EnrollCourseDto enrollDto)
        {
            // [Authorize(Roles = "Student")] đảm bảo userId luôn có
            var userId = User.GetUserId();
            var result = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // endpoint Student hủy đăng ký khóa học
        [HttpDelete("course/{courseId}")]
        public async Task<IActionResult> UnenrollFromCourse(int courseId)
        {
            // [Authorize(Roles = "Student")] đảm bảo userId luôn có
            var userId = User.GetUserId();
            var result = await _userEnrollmentService.UnenrollFromCourseAsync(courseId, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // endpoint Student lấy danh sách khóa học đã đăng ký (chỉ phân trang, không filter)
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyEnrolledCourses([FromQuery] PageRequest request)
        {
            // [Authorize(Roles = "Student")] đảm bảo userId luôn có
            var userId = User.GetUserId();
            var result = await _enrollmentQueryService.GetMyEnrolledCoursesPagedAsync(userId, request);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // endpoint Student tham gia khóa học qua mã lớp
        [HttpPost("join-by-class-code")]
        public async Task<IActionResult> JoincourseByClassCode([FromBody] EnrollCourseByClassCodeDto joinDto)
        {
            // [Authorize(Roles = "Student")] đảm bảo userId luôn có
            var userId = User.GetUserId();
            var result = await _userEnrollmentService.EnrollInCourseByClassCodeAsync(joinDto.ClassCode, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }


    }
}
