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
        public async Task<ServiceResponse<IEnumerable<CourseDto>>> GetAllCoursesAsync()
        {
            var response = new ServiceResponse<IEnumerable<CourseDto>>();

            try
            {
                var courses = await _courseRepository.GetAllCourses();
                var courseDtos = new List<CourseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<CourseDto>(course);

                    // Thêm thống kê
                    courseDto.LessonCount = await _courseRepository.CountLessons(course.CourseId);
                    courseDto.StudentCount = await _courseRepository.CountEnrolledUsers(course.CourseId);
                    courseDto.TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "Unknown";

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
        /// Lấy chi tiết khóa học
        /// </summary>
        public async Task<ServiceResponse<CourseDetailDto>> GetCourseDetailAsync(int courseId, int? userId = null)
        {
            var response = new ServiceResponse<CourseDetailDto>();

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

                var courseDetailDto = _mapper.Map<CourseDetailDto>(course);

                // Thêm thông tin bổ sung
                courseDetailDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseDetailDto.TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "Unknown";

                // Kiểm tra user đã đăng ký chưa
                if (userId.HasValue)
                {
                    courseDetailDto.IsEnrolled = await _courseRepository.IsUserEnrolledInCourse(userId.Value, courseId);
                }

                // Map lessons
                courseDetailDto.Lessons = course.Lessons?.Select(l => new LessonDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description
                }).ToList();

                response.Data = courseDetailDto;
                response.Message = "Course details retrieved successfully";

                _logger.LogInformation("Retrieved course details for CourseId: {CourseId}", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving course details: {ex.Message}";
                _logger.LogError(ex, "Error in GetCourseDetailAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        // === TEACHER METHODS ===

        /// <summary>
        /// Teacher - Tạo khóa học mới
        /// </summary>
        public async Task<ServiceResponse<CourseDto>> CreateCourseAsync(CreateCourseDto courseDto)
        {
            var response = new ServiceResponse<CourseDto>();

            try
            {
                if (courseDto == null)
                {
                    response.Success = false;
                    response.Message = "Course data is required";
                    return response;
                }

                // Kiểm tra teacher tồn tại
                var teacher = await _userRepository.GetByIdAsync(courseDto.TeacherId);
                if (teacher == null)
                {
                    response.Success = false;
                    response.Message = "Teacher not found";
                    return response;
                }

                var course = _mapper.Map<Course>(courseDto);

                await _courseRepository.AddCourse(course);

                var createdCourseDto = _mapper.Map<CourseDto>(course);
                createdCourseDto.TeacherName = teacher.SureName + " " + teacher.LastName;
                createdCourseDto.LessonCount = 0;
                createdCourseDto.StudentCount = 0;

                response.Data = createdCourseDto;
                response.Message = "Course created successfully";

                _logger.LogInformation("Teacher {TeacherId} created course: {CourseTitle}", courseDto.TeacherId, courseDto.Title);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error creating course: {ex.Message}";
                _logger.LogError(ex, "Error in CreateCourseAsync");
            }

            return response;
        }

        /// <summary>
        /// Teacher - Lấy danh sách khóa học của mình
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<ListMyCourseTeacherDto>>> GetMyCoursesByTeacherAsync(int teacherId)
        {
            var response = new ServiceResponse<IEnumerable<ListMyCourseTeacherDto>>();

            try
            {
                if (teacherId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid teacher ID";
                    return response;
                }

                var courses = await _courseRepository.GetAllCoursesByTeacherId(teacherId);
                var courseDtos = new List<ListMyCourseTeacherDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<ListMyCourseTeacherDto>(course);
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
                if (joinDto == null || joinDto.CourseId <= 0 || teacherId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid data provided";
                    return response;
                }

                // Kiểm tra khóa học tồn tại
                var course = await _courseRepository.GetCourseById(joinDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra không thể tham gia khóa học của chính mình
                if (course.TeacherId == teacherId)
                {
                    response.Success = false;
                    response.Message = "Cannot join your own course";
                    return response;
                }

                // Kiểm tra đã tham gia chưa
                var isEnrolled = await _courseRepository.IsUserEnrolledInCourse(teacherId, joinDto.CourseId);
                if (isEnrolled)
                {
                    response.Success = false;
                    response.Message = "Already joined this course";
                    return response;
                }

                await _courseRepository.EnrollUserInCourse(teacherId, joinDto.CourseId);

                response.Data = true;
                response.Message = "Successfully joined the course";

                _logger.LogInformation("Teacher {TeacherId} joined course {CourseId}", teacherId, joinDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error joining course: {ex.Message}";
                _logger.LogError(ex, "Error in JoinCourseAsTeacherAsync");
            }

            return response;
        }

        // === USER/STUDENT METHODS ===

        /// <summary>
        /// User - Lấy danh sách khóa học hệ thống
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<UserCourseDto>>> GetSystemCoursesAsync(int? userId = null)
        {
            var response = new ServiceResponse<IEnumerable<UserCourseDto>>();

            try
            {
                var courses = await _courseRepository.GetAllCourseSystem();
                var courseDtos = new List<UserCourseDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<UserCourseDto>(course);

                    // Kiểm tra user đã đăng ký chưa
                    if (userId.HasValue)
                    {
                        courseDto.IsEnrolled = await _courseRepository.IsUserEnrolledInCourse(userId.Value, course.CourseId);
                    }

                    courseDtos.Add(courseDto);
                }

                response.Data = courseDtos;
                response.Message = "Retrieved system courses successfully";

                _logger.LogInformation("Retrieved {Count} system courses for user {UserId}", courseDtos.Count, userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving system courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetSystemCoursesAsync");
            }

            return response;
        }

        /// <summary>
        /// Student - Lấy danh sách khóa học đã đăng ký
        /// </summary>
        public async Task<ServiceResponse<IEnumerable<ListMyCourseStudentDto>>> GetMyEnrolledCoursesAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<ListMyCourseStudentDto>>();

            try
            {
                if (userId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid user ID";
                    return response;
                }

                var courses = await _courseRepository.GetEnrolledCoursesByUserId(userId);
                var courseDtos = courses.Select(course =>
                {
                    var dto = _mapper.Map<ListMyCourseStudentDto>(course);
                    dto.TeacherName = course.Teacher?.SureName + " " + course.Teacher?.LastName ?? "Unknown";
                    return dto;
                });

                response.Data = courseDtos;
                response.Message = "Retrieved enrolled courses successfully";

                _logger.LogInformation("User {UserId} retrieved {Count} enrolled courses", userId, courseDtos.Count());
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
        /// Student - Đăng ký khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> EnrollInCourseAsync(EnrollCourseDto enrollDto, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (enrollDto == null || enrollDto.CourseId <= 0 || userId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid data provided";
                    return response;
                }

                // Kiểm tra khóa học tồn tại
                var course = await _courseRepository.GetCourseById(enrollDto.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Kiểm tra đã đăng ký chưa
                var isEnrolled = await _courseRepository.IsUserEnrolledInCourse(userId, enrollDto.CourseId);
                if (isEnrolled)
                {
                    response.Success = false;
                    response.Message = "Already enrolled in this course";
                    return response;
                }

                await _courseRepository.EnrollUserInCourse(userId, enrollDto.CourseId);

                response.Data = true;
                response.Message = "Successfully enrolled in the course";

                _logger.LogInformation("User {UserId} enrolled in course {CourseId}", userId, enrollDto.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error enrolling in course: {ex.Message}";
                _logger.LogError(ex, "Error in EnrollInCourseAsync");
            }

            return response;
        }

        /// <summary>
        /// Student - Hủy đăng ký khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> UnenrollFromCourseAsync(int courseId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (courseId <= 0 || userId <= 0)
                {
                    response.Success = false;
                    response.Message = "Invalid course ID or user ID";
                    return response;
                }

                // Kiểm tra đã đăng ký chưa
                var isEnrolled = await _courseRepository.IsUserEnrolledInCourse(userId, courseId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.Message = "Not enrolled in this course";
                    return response;
                }

                await _courseRepository.UnenrollUserFromCourse(userId, courseId);

                response.Data = true;
                response.Message = "Successfully unenrolled from the course";

                _logger.LogInformation("User {UserId} unenrolled from course {CourseId}", userId, courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error unenrolling from course: {ex.Message}";
                _logger.LogError(ex, "Error in UnenrollFromCourseAsync");
            }

            return response;
        }

        // === UTILITY METHODS ===

        /// <summary>
        /// Cập nhật khóa học
        /// </summary>
        public async Task<ServiceResponse<bool>> UpdateCourseAsync(int courseId, CreateCourseDto courseDto)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                if (courseId <= 0 || courseDto == null)
                {
                    response.Success = false;
                    response.Message = "Invalid data provided";
                    return response;
                }

                var existingCourse = await _courseRepository.GetCourseById(courseId);
                if (existingCourse == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                // Cập nhật thông tin
                existingCourse.Title = courseDto.Title;
                existingCourse.Description = courseDto.Description;
                existingCourse.Img = courseDto.Img;
                existingCourse.Type = courseDto.Type;
                existingCourse.Price = courseDto.Price;

                await _courseRepository.UpdateCourse(existingCourse);

                response.Data = true;
                response.Message = "Course updated successfully";

                _logger.LogInformation("Course {CourseId} updated successfully", courseId);
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
        /// Xóa khóa học
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

                var course = await _courseRepository.GetCourseById(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.Message = "Course not found";
                    return response;
                }

                await _courseRepository.DeleteCourse(courseId);

                response.Data = true;
                response.Message = "Course deleted successfully";

                _logger.LogInformation("Course {CourseId} deleted successfully", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error deleting course: {ex.Message}";
                _logger.LogError(ex, "Error in DeleteCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }
    }
}
