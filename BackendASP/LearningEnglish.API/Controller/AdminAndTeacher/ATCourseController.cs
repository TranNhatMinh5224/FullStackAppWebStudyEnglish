using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.API.Extensions;
using LearningEnglish.API.Authorization;

namespace LearningEnglish.API.Controller.AdminAndTeacher
{
    [ApiController]
    [Route("api/courses")]
    [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
    public class CourseController : ControllerBase
    {
        private readonly IAdminCourseService _adminCourseService;
        private readonly ITeacherCourseService _teacherCourseService;
        private readonly IUserEnrollmentService _userEnrollmentService;
        private readonly ILogger<CourseController> _logger;
        private readonly IUserManagementService _userManagementService;

        public CourseController(
            IAdminCourseService adminCourseService,
            ITeacherCourseService teacherCourseService,
            IUserEnrollmentService userEnrollmentService,
            ILogger<CourseController> logger,
            IUserManagementService userManagementService)
        {
            _adminCourseService = adminCourseService;
            _teacherCourseService = teacherCourseService;
            _userEnrollmentService = userEnrollmentService;
            _logger = logger;
            _userManagementService = userManagementService;
        }

        // endpoint Admin lấy danh sách loại khóa học để filter (System/Teacher)
        // Dùng cho giao diện quản lý: render dropdown filter để lọc danh sách khóa học theo Type
        [HttpGet("types")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseTypes()
        {
            var result = await _adminCourseService.GetCourseTypesAsync();
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin lấy danh sách tất cả khóa học
        [HttpGet("admin/all")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> GetAllCourses([FromQuery] AdminCourseQueryParameters request)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("🔐 Admin {UserId} đang lấy danh sách courses", userId);
            
            // PageRequest có giá trị mặc định (PageNumber=1, PageSize=20), luôn dùng phân trang
            var pagedResult = await _adminCourseService.GetAllCoursesPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // endpoint Admin xóa khóa học
        [HttpDelete("admin/{courseId}")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> AdminDeleteCourse(int courseId)
        {
            var userId = User.GetUserId();
            _logger.LogInformation("Admin {UserId} đang xóa course {CourseId}", userId, courseId);

            var result = await _adminCourseService.DeleteCourseAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher xóa khóa học của mình
        [HttpDelete("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDeleteCourse(int courseId)
        {
            var teacherId = User.GetUserId();
            _logger.LogInformation("Teacher {TeacherId} đang xóa course {CourseId}", teacherId, courseId);

            var result = await _teacherCourseService.DeleteCourseAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin tạo khóa học
        [HttpPost("admin/create")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> AdminCreateCourse([FromBody] AdminCreateCourseRequestDto requestDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation(
                " Admin {UserId} đang tạo course mới: {Title}",
                userId, requestDto.Title);

            var result = await _adminCourseService.AdminCreateCourseAsync(requestDto);
            
            if (result.Success)
            {
                _logger.LogInformation(
                    " Admin {UserId} tạo course thành công: CourseId={CourseId}, Title={Title}",
                    userId, result.Data?.CourseId, requestDto.Title);
            }
            else
            {
                _logger.LogWarning(
                    "admin {UserId} tạo course thất bại: {Message}",
                    userId, result.Message);
            }

            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher tạo khóa học
        [HttpPost("teacher/create")]
        [RequireTeacherRole] // Kiểm tra role Teacher trong database (không tin JWT token)
        public async Task<IActionResult> CreateCourse([FromBody] TeacherCreateCourseRequestDto requestDto)
        {

            var teacherId = User.GetUserId();
            var result = await _teacherCourseService.CreateCourseAsync(requestDto, teacherId);
            return result.Success
                ? CreatedAtAction(null, new { courseId = result.Data?.CourseId }, result)
                : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher lấy danh sách khóa học của mình (chỉ phân trang, không filter)
        [HttpGet("teacher/my-courses")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetMyCourses([FromQuery] PageRequest request)
        {
            // PageRequest có giá trị mặc định (PageNumber=1, PageSize=20), luôn dùng phân trang
            var pagedResult = await _teacherCourseService.GetMyCoursesPagedAsync(request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // endpoint Teacher xem chi tiết khóa học của mình
        [HttpGet("teacher/{courseId}/detail")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> GetCourseDetail(int courseId)
        {
            var result = await _teacherCourseService.GetCourseDetailAsync(courseId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin cập nhật khóa học
        [HttpPut("admin/{courseId}")]
        [RequirePermission("Admin.Course.Manage")]
        public async Task<IActionResult> AdminUpdateCourse(int courseId, [FromBody] AdminUpdateCourseRequestDto requestDto)
        {
            var userId = User.GetUserId();
            _logger.LogInformation(" Admin {UserId} đang sửa course {CourseId}", userId, courseId);

            var result = await _adminCourseService.AdminUpdateCourseAsync(courseId, requestDto);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher cập nhật khóa học của mình
        [HttpPut("teacher/{courseId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> UpdateCourse(int courseId, [FromBody] TeacherUpdateCourseRequestDto requestDto)
        {

            var teacherId = User.GetUserId();
            var result = await _teacherCourseService.UpdateCourseAsync(courseId, requestDto, teacherId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin/Teacher xem danh sách học viên trong khóa học
        [HttpGet("{courseId}/students")]
        [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
        public async Task<IActionResult> GetUsersByCourseId(int courseId, [FromQuery] PageRequest request)
        {
            var userId = User.GetUserIdSafe(); // Chỉ để log, không truyền vào service
            _logger.LogInformation("User {UserId} đang lấy danh sách students trong course {CourseId}", userId, courseId);

            // PageRequest có giá trị mặc định, luôn dùng phân trang
            // RLS đã filter, không cần userId trong service
            var pagedResult = await _userManagementService.GetUsersByCourseIdPagedAsync(courseId, request);
            return pagedResult.Success ? Ok(pagedResult) : StatusCode(pagedResult.StatusCode, pagedResult);
        }

        // endpoint Admin/Teacher xem chi tiết học viên trong khóa học
        [HttpGet("{courseId}/students/{studentId}")]
        [Authorize(Roles = "SuperAdmin, ContentAdmin, FinanceAdmin, Teacher")]
        public async Task<IActionResult> GetStudentDetailInCourse(int courseId, int studentId)
        {
            var userId = User.GetUserIdSafe(); // Chỉ để log, không truyền vào service
            _logger.LogInformation("User {UserId} đang xem chi tiết student {StudentId} trong course {CourseId}", userId, studentId, courseId);

            // RLS đã filter, không cần userId trong service
            var result = await _userManagementService.GetStudentDetailInCourseAsync(courseId, studentId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin thêm học viên vào khóa học (bằng email)
        [HttpPost("admin/{courseId}/students")]
        [RequirePermission("Admin.Course.Manage", "Admin.Course.Enroll")]
        public async Task<IActionResult> AdminAddStudentToCourse(int courseId, [FromBody] AddStudentToCourseDto request)
        {
            var userId = User.GetUserId(); // Cần userId để log và audit (throw exception nếu không có)
            _logger.LogInformation("Admin {UserId} đang thêm student {Email} vào course {CourseId}", userId, request.Email, courseId);

            var result = await _userManagementService.AddStudentToCourseByEmailAsync(courseId, request.Email, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher thêm học viên vào khóa học của mình (bằng email)
        [HttpPost("teacher/{courseId}/students")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherAddStudentToCourse(int courseId, [FromBody] AddStudentToCourseDto request)
        {
            var userId = User.GetUserId(); // Cần userId để log và audit (throw exception nếu không có)
            _logger.LogInformation("Teacher {UserId} đang thêm student {Email} vào course {CourseId}", userId, request.Email, courseId);

            var result = await _userManagementService.AddStudentToCourseByEmailAsync(courseId, request.Email, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Admin xóa học viên khỏi khóa học
        [HttpDelete("admin/{courseId}/students/{studentId}")]
        [RequirePermission("Admin.Course.Manage", "Admin.Course.Enroll")]
        public async Task<IActionResult> AdminRemoveStudentFromCourse(int courseId, int studentId)
        {
            var userId = User.GetUserId(); // Cần userId để log và audit (throw exception nếu không có)
            _logger.LogInformation("Admin {UserId} đang xóa student {StudentId} khỏi course {CourseId}", userId, studentId, courseId);

            var result = await _userManagementService.RemoveStudentFromCourseAsync(courseId, studentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }

        // endpoint Teacher xóa học viên khỏi khóa học của mình
        [HttpDelete("teacher/{courseId}/students/{studentId}")]
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherRemoveStudentFromCourse(int courseId, int studentId)
        {
            var userId = User.GetUserId(); // Cần userId để log và audit (throw exception nếu không có)
            _logger.LogInformation("Teacher {UserId} đang xóa student {StudentId} khỏi course {CourseId}", userId, studentId, courseId);

            var result = await _userManagementService.RemoveStudentFromCourseAsync(courseId, studentId, userId);
            return result.Success ? Ok(result) : StatusCode(result.StatusCode, result);
        }
    }
}

