using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.AdminManagement;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;



//  Giáo Viên   Quản Lý người dùng trong khóa học của giáo viên 






namespace LearningEnglish.API.Controller.Teacher
{
    [ApiController]
    [Route("api/teacher/courses")]
    [RequireTeacherRole]
    public class TeacherManageUserInCourseController : ControllerBase
    {
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly IUserManagementService _userManagementService;
        private readonly IManageUserInCourseService _manageUserInCourseService;
        private readonly ILogger<TeacherManageUserInCourseController> _logger;

        public TeacherManageUserInCourseController(
            ITeacherCourseService teacherCourseService,
            IUserManagementService userManagementService,
            IManageUserInCourseService manageUserInCourseService,
            ILogger<TeacherManageUserInCourseController> logger)
        {
            _teacherCourseService = teacherCourseService;
            _userManagementService = userManagementService;
            _manageUserInCourseService = manageUserInCourseService;
            _logger = logger;
        }

        // GET: Teacher lấy danh sách học viên trong khóa học của mình
        [HttpGet("{courseId}/students")]
        public async Task<IActionResult> GetStudentsInCourse(int courseId, [FromQuery] PageRequest request)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation(
                "Teacher {TeacherId} lấy students của course {CourseId}",
                teacherId, courseId
            );

            var result = await _manageUserInCourseService.GetUsersByCourseIdPagedAsync(courseId, teacherId, request);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // GET: Teacher xem chi tiết học viên trong khóa học của mình
        [HttpGet("{courseId}/students/{studentId}")]
        public async Task<IActionResult> GetStudentDetail(int courseId, int studentId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation(
                "Teacher {TeacherId} xem student {StudentId} trong course {CourseId}",
                teacherId, studentId, courseId
            );

            var result = await _manageUserInCourseService.GetStudentDetailInCourseAsync(courseId, studentId, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // POST: Teacher thêm học viên vào khóa học của mình
        [HttpPost("{courseId}/students")]
        public async Task<IActionResult> AddStudentToCourse(int courseId, [FromBody] AddStudentToCourseDto request)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation(
                "Teacher {TeacherId} thêm student {Email} vào course {CourseId}",
                teacherId, request.Email, courseId
            );

            var result = await _manageUserInCourseService.AddStudentToCourseByEmailAsync(
                courseId, request.Email, teacherId
            );

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // DELETE: Teacher xóa học viên khỏi khóa học của mình
        [HttpDelete("{courseId}/students/{studentId}")]
        public async Task<IActionResult> RemoveStudentFromCourse(int courseId, int studentId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation(
                "Teacher {TeacherId} xóa student {StudentId} khỏi course {CourseId}",
                teacherId, studentId, courseId
            );

            var result = await _manageUserInCourseService.RemoveStudentFromCourseAsync(
                courseId, studentId, teacherId
            );

            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}
