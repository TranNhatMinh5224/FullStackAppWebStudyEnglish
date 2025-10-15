using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseService> _logger;

        public CourseService(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<CourseService> logger)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // === ADMIN METHODS ===

        /// <summary>
        /// Admin - Lấy tất cả khóa học với thống kê
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<AdminCourseListResponseDto>>> GetAllCoursesAsync()
        {
            var response = new ServiceResponse<IEnumerable<AdminCourseListResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetAllCourses();
                var courseDtos = new List<AdminCourseListResponseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<AdminCourseListResponseDto>(course);

                    // Thêm thống kê
                    courseDto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    courseDto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);
                    courseDto.TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "System Admin";

                    courseDtos.Add(courseDto);
                }

                response.Data = courseDtos;
                response.Message = "Retrieved all courses successfully";

                _logger.LogInformation("Admin retrieved {Count} courses", courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetAllCoursesAsync");
            }

            return response;
        }

        /// <summary>
        /// Admin - Tạo khóa học mới (System course)
        /// </summary>
        public async Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                if (requestDto == null)
                {
                    response.Success = false;
                    response.Message = "Course data is required";
                    return response;
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(requestDto.Title))
                {
                    response.Success = false;
                    response.Message = "Course title is required";
                    return response;
                }

                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Img = requestDto.Img,
                    Type = requestDto.Type,
                    Price = null, // Admin có thể set price sau
                    TeacherId = null // System course không có Teacher
                };

                await _courseRepository.AddCourse(course);

                // Tạo response DTO
                var courseResponseDto = new CourseResponseDto
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    Img = course.Img,
                    Type = course.Type,
                    Price = course.Price,
                    TeacherId = course.TeacherId,
                    TeacherName = "System Admin",
                    LessonCount = 0,
                    StudentCount = 0
                };

                response.Data = courseResponseDto;
                response.Message = "Course created successfully by Admin";

                _logger.LogInformation("Admin created course: {CourseTitle} (ID: {CourseId})", requestDto.Title, course.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
                _logger.LogError(ex, "Error in AdminCreateCourseAsync");
            }

            return response;
        }

        /// <summary>
        /// Admin - Xóa khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (courseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                await _courseRepository.DeleteCourse(courseId);

                response.Success = true;
                response.Data = true;
                response.Message = "Course deleted successfully";

                _logger.LogInformation("Course {CourseId} deleted", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting course: {ex.Message}";
                _logger.LogError(ex, "Error in DeleteCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        /// <summary>
        /// Lấy chi tiết khóa học
        /// </summary>
        public async Task<ServiceResponse<CourseDetailResponseDto>> GetCourseDetailAsync(int courseId, int? userId = null)
        {
            var response = new ServiceResponse<CourseDetailResponseDto>();

            try
            {
                if (courseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                var course = await _courseRepository.GetCourseWithDetails(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                var courseDetailDto = _mapper.Map<CourseDetailResponseDto>(course);

                // Thêm thông tin bổ sung
                courseDetailDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseDetailDto.TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "System Admin";

                // Kiểm tra user đã đăng ký chưa
                if (userId.HasValue)
                {
                    courseDetailDto.IsEnrolled = await _courseRepository.IsUserEnrolled(courseId, userId.Value);
                }

                response.Data = courseDetailDto;
                response.Message = "Course detail retrieved successfully";

                _logger.LogInformation("Retrieved course detail for CourseId: {CourseId}", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course detail: {ex.Message}";
                _logger.LogError(ex, "Error in GetCourseDetailAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        // === TEACHER METHODS ===

        /// <summary>
        /// Teacher - Tạo khóa học mới
        /// </summary>
        public async Task<ServiceResponse<CourseResponseDto>> CreateCourseAsync(TeacherCreateCourseRequestDto requestDto, int teacherId)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                if (requestDto == null)
                {
                    response.Success = false;
                    response.Message = "Course data is required";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(requestDto.Title))
                {
                    response.Success = false;
                    response.Message = "Course title is required";
                    return response;
                }

                // Kiểm tra teacher tồn tại
                var teacher = await _userRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    response.Success = false;
                    response.Message = "Teacher not found";
                    return response;
                }

                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Img = requestDto.Img,
                    Type = requestDto.Type,
                    Price = requestDto.Price,
                    TeacherId = teacherId
                };

                await _courseRepository.AddCourse(course);

                // Tạo response DTO
                var courseResponseDto = new CourseResponseDto
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    Img = course.Img,
                    Type = course.Type,
                    Price = course.Price,
                    TeacherId = course.TeacherId,
                    TeacherName = $"{teacher.SureName} {teacher.LastName}",
                    LessonCount = 0,
                    StudentCount = 0
                };

                response.Data = courseResponseDto;
                response.Message = "Course created successfully by Teacher";

                _logger.LogInformation("Teacher {TeacherId} created course: {CourseTitle} (ID: {CourseId})", teacherId, requestDto.Title, course.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
                _logger.LogError(ex, "Error in CreateCourseAsync for TeacherId: {TeacherId}", teacherId);
            }

            return response;
        }

        /// <summary>
        /// Teacher - Cập nhật khóa học
        /// </summary>
        public async Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(int courseId, TeacherCreateCourseRequestDto requestDto, int teacherId)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                if (courseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra quyền sở hữu
                if (course.TeacherId != teacherId)
                {
                    response.Success = false;
                    response.Message = "You don't have permission to update this course";
                    return response;
                }

                // Cập nhật course
                course.Title = requestDto.Title;
                course.Description = requestDto.Description;
                course.Img = requestDto.Img;
                course.Type = requestDto.Type;
                course.Price = requestDto.Price;

                await _courseRepository.UpdateCourse(course);

                var courseResponseDto = new CourseResponseDto
                {
                    CourseId = course.CourseId,
                    Title = course.Title,
                    Description = course.Description,
                    Img = course.Img,
                    Type = course.Type,
                    Price = course.Price,
                    TeacherId = course.TeacherId,
                    TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "Unknown",
                    LessonCount = await _courseRepository.CountLessons(courseId),
                    StudentCount = await _courseRepository.CountEnrolledUsers(courseId)
                };

                response.Data = courseResponseDto;
                response.Message = "Course updated successfully";

                _logger.LogInformation("Course {CourseId} updated by Teacher {TeacherId}", courseId, teacherId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error updating course: {ex.Message}";
                _logger.LogError(ex, "Error in UpdateCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        /// <summary>
        /// Teacher - Lấy danh sách khóa học của mình
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyCoursesByTeacherAsync(int teacherId)
        {
            var response = new ServiceResponse<IEnumerable<CourseResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetCoursesByTeacher(teacherId);
                var courseDtos = new List<CourseResponseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseResponseDto>(course);
                    courseDto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    courseDto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);

                    courseDtos.Add(courseDto);
                }

                response.Data = courseDtos;
                response.Message = "Retrieved teacher's courses successfully";

                _logger.LogInformation("Teacher {TeacherId} retrieved {Count} courses", teacherId, courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving teacher's courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetMyCoursesByTeacherAsync for TeacherId: {TeacherId}", teacherId);
            }

            return response;
        }

        /// <summary>
        /// Teacher - Tham gia khóa học của teacher khác
        /// </summary>
        public async Task<ServiceResponse<bool>> JoinCourseAsTeacherAsync(JoinCourseTeacherDto joinDto, int teacherId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (joinDto == null || joinDto.CourseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(joinDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra course có phải Teacher course không
                if (course.Type != Domain.Enums.CourseType.Teacher)
                {
                    response.Success = false;
                    response.Message = "Can only join Teacher courses";
                    return response;
                }

                // Logic tham gia course (có thể implement sau)
                response.Success = true;
                response.Data = true;
                response.Message = "Successfully joined course as teacher";

                _logger.LogInformation("Teacher {TeacherId} joined course {CourseId}", teacherId, joinDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error joining course: {ex.Message}";
                _logger.LogError(ex, "Error in JoinCourseAsTeacherAsync for TeacherId: {TeacherId}, CourseId: {CourseId}", teacherId, joinDto.CourseId);
            }

            return response;
        }

        // === USER/STUDENT METHODS ===

        /// <summary>
        /// User - Lấy danh sách khóa học System
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<UserCourseListResponseDto>>> GetSystemCoursesAsync(int? userId = null)
        {
            var response = new ServiceResponse<IEnumerable<UserCourseListResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetSystemCourses();
                var courseDtos = new List<UserCourseListResponseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<UserCourseListResponseDto>(course);
                    courseDto.TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "System Admin";

                    // Kiểm tra user đã đăng ký chưa
                    if (userId.HasValue)
                    {
                        courseDto.IsEnrolled = await _courseRepository.IsUserEnrolled(course.CourseId, userId.Value);
                    }

                    courseDtos.Add(courseDto);
                }

                response.Data = courseDtos;
                response.Message = "Retrieved system courses successfully";

                _logger.LogInformation("Retrieved {Count} system courses for UserId: {UserId}", courseDtos.Count, userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving system courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetSystemCoursesAsync for UserId: {UserId}", userId);
            }

            return response;
        }

        /// <summary>
        /// User - Lấy danh sách khóa học đã đăng ký
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyEnrolledCoursesAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<CourseResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetEnrolledCoursesByUser(userId);
                var courseDtos = new List<CourseResponseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseResponseDto>(course);
                    courseDto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    courseDto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);

                    courseDtos.Add(courseDto);
                }

                response.Data = courseDtos;
                response.Message = "Retrieved enrolled courses successfully";

                _logger.LogInformation("User {UserId} retrieved {Count} enrolled courses", userId, courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving enrolled courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetMyEnrolledCoursesAsync for UserId: {UserId}", userId);
            }

            return response;
        }

        /// <summary>
        /// User - Đăng ký khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (enrollDto == null || enrollDto.CourseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                // Kiểm tra course tồn tại
                var course = await _courseRepository.GetByIdAsync(enrollDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra user đã đăng ký chưa
                if (await _courseRepository.IsUserEnrolled(enrollDto.CourseId, userId))
                {
                    response.Success = false;
                    response.Message = "User already enrolled in this course";
                    return response;
                }

                // Đăng ký user vào course
                await _courseRepository.EnrollUserInCourse(enrollDto.CourseId, userId);

                response.Success = true;
                response.Data = true;
                response.Message = "Successfully enrolled in course";

                _logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, enrollDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error enrolling in course: {ex.Message}";
                _logger.LogError(ex, "Error in EnrollInCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, enrollDto.CourseId);
            }

            return response;
        }

        /// <summary>
        /// User - Hủy đăng ký khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (courseId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID";
                    return response;
                }

                // Kiểm tra user đã đăng ký chưa
                if (!await _courseRepository.IsUserEnrolled(courseId, userId))
                {
                    response.Success = false;
                    response.Message = "User is not enrolled in this course";
                    return response;
                }

                // Hủy đăng ký
                await _courseRepository.UnenrollUserFromCourse(courseId, userId);

                response.Success = true;
                response.Data = true;
                response.Message = "Successfully unenrolled from course";

                _logger.LogInformation("User {UserId} unenrolled from course {CourseId}", userId, courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error unenrolling from course: {ex.Message}";
                _logger.LogError(ex, "Error in UnenrollFromCourseAsync for UserId: {UserId}, CourseId: {CourseId}", userId, courseId);
            }

            return response;
        }
    }
}
