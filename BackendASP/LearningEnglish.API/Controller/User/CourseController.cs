using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using System.Security.Claims;

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

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // POST: api/user/enroll/course - Đăng ký khóa học
        [HttpPost("course")]
        public async Task<IActionResult> EnrollInCourse([FromBody] EnrollCourseDto enrollDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _userEnrollmentService.EnrollInCourseAsync(enrollDto, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // DELETE: api/user/enroll/course/{courseId} - Hủy đăng ký khóa học
        [HttpDelete("course/{courseId}")]
        public async Task<IActionResult> UnenrollFromCourse(int courseId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _userEnrollmentService.UnenrollFromCourseAsync(courseId, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // GET: api/user/enroll/my-courses - Lấy danh sách khóa học đã đăng ký với phân trang
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyEnrolledCourses([FromQuery] PageRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _enrollmentQueryService.GetMyEnrolledCoursesPagedAsync(userId, request);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }

        // POST: api/user/enroll/join-by-class-code - Tham gia khóa học qua mã lớp
        [HttpPost("join-by-class-code")]
        public async Task<IActionResult> JoincourseByClassCode([FromBody] EnrollCourseByClassCodeDto joinDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId == 0)
                return Unauthorized(new { message = "Invalid user credentials" });

            var result = await _userEnrollmentService.EnrollInCourseByClassCodeAsync(joinDto.ClassCode, userId);

            return result.Success
                ? Ok(result)
                : StatusCode(result.StatusCode, result);
        }


    }
}
