using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Application.Common;
using CleanDemo.Application.Common.Utils;
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
        private readonly ITeacherPackageRepository _teacherPackageRepository;

        public CourseService(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<CourseService> logger,
            ITeacherPackageRepository teacherPackageRepository)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _teacherPackageRepository = teacherPackageRepository;
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
                    courseDto.TeacherName = course.Teacher?.FirstName + " " + course.Teacher?.LastName ?? "System Admin";

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



                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Img = requestDto.Img,
                    Type = requestDto.Type,
                    Price = requestDto.Price,
                    TeacherId = null
                };

                await _courseRepository.AddCourse(course);

                // Dùng Mapper thay manual
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.TeacherName = "System Admin";
                courseResponseDto.LessonCount = 0;
                courseResponseDto.StudentCount = 0;

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
                courseDetailDto.TeacherName = course.Teacher?.FirstName + " " + course.Teacher?.LastName ?? "System Admin";

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
            var classCode = ClassCodeGenerator.Generate();

            try
            {


                // Kiểm tra teacher tồn tại
                var teacher = await _userRepository.GetByIdAsync(teacherId);
                if (teacher == null)
                {
                    response.Success = false;
                    response.Message = "Teacher not found";
                    return response;
                }

                // Kiểm tra teacher có subscription active không
                var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(teacherId);
                if (teacherPackage == null)
                {
                    response.Success = false;
                    response.Message = "You need an active subscription to create courses";
                    return response;
                }

                // Kiểm tra số lượng course hiện tại
                var teacherCourses = await _courseRepository.GetCoursesByTeacher(teacherId);
                int currentCourseCount = teacherCourses.Count();
                int maxCourses = teacherPackage.MaxCourses;

                if (currentCourseCount >= maxCourses)
                {
                    response.Success = false;
                    response.Message = $"You have reached the maximum number of courses ({currentCourseCount}/{maxCourses}). Please upgrade your package.";
                    return response;
                }

                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Img = requestDto.Img,
                    Type = requestDto.Type,

                    TeacherId = teacherId,
                    ClassCode = classCode
                };

                await _courseRepository.AddCourse(course);

                // Dùng Mapper thay manual
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = 0;
                courseResponseDto.StudentCount = 0;

                response.Data = courseResponseDto;
                response.Message = $"Course created successfully ({currentCourseCount + 1}/{maxCourses} courses)";

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


                await _courseRepository.UpdateCourse(course);

                // Dùng Mapper thay manual
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseResponseDto.StudentCount = await _courseRepository.CountEnrolledUsers(courseId);

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
                    courseDto.TeacherName = course.Teacher?.FirstName + " " + course.Teacher?.LastName ?? "System Admin";

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


    }
}
