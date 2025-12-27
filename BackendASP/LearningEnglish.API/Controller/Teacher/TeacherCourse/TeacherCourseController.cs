using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;




// CRUD khóa học của giáo viên




namespace LearningEnglish.API.Controller.Teacher
{
    [ApiController]
    [Route("api/teacher/courses")]
    [RequireTeacherRole]
    public class TeacherCourseController : ControllerBase
    {
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<TeacherCourseController> _logger;

        public TeacherCourseController(
            ITeacherCourseService teacherCourseService,
            IUserManagementService userManagementService,
            ILogger<TeacherCourseController> logger)
        {
            _teacherCourseService = teacherCourseService;
            _userManagementService = userManagementService;
            _logger = logger;
        }

        // GET: Teacher lấy danh sách khóa học của mình
        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses([FromQuery] PageRequest request)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} lấy danh sách own courses", teacherId);

            var result = await _teacherCourseService.GetMyCoursesPagedAsync(request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Teacher xem chi tiết khóa học của mình
        [HttpGet("{courseId}")]
        public async Task<IActionResult> GetCourseDetail(int courseId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} xem chi tiết course {CourseId}", teacherId, courseId);

            var result = await _teacherCourseService.GetCourseDetailAsync(courseId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: Teacher tạo khóa học mới
        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] TeacherCreateCourseRequestDto requestDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} tạo course: {Title}", teacherId, requestDto.Title);

            var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);
            return result.Success
                ? CreatedAtAction(nameof(GetCourseDetail), new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // PUT: Teacher cập nhật khóa học của mình
        [HttpPut("{courseId}")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherUpdateCourseRequestDto requestDto)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} cập nhật course {CourseId}", teacherId, courseId);

            var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Teacher xóa khóa học của mình
        [HttpDelete("{courseId}")]
        public async Task<IActionResult> DeleteCourse(int courseId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} xóa course {CourseId}", teacherId, courseId);

            var result = await _teacherCourseService.DeleteCourseAsync(courseId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
