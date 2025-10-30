using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Domain.Entities;
using CleanDemo.Application.Common;
using CleanDemo.Application.Common.Utils;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class TeacherCourseService : ITeacherCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherCourseService> _logger;
        private readonly ITeacherPackageRepository _teacherPackageRepository;

        public TeacherCourseService(
            ICourseRepository courseRepository,
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<TeacherCourseService> logger,
            ITeacherPackageRepository teacherPackageRepository)
        {
            _courseRepository = courseRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
            _teacherPackageRepository = teacherPackageRepository;
        }

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

                // Kiểm tra MaxStudent của course không vượt quá giới hạn package
                if (requestDto.MaxStudent > 0 && requestDto.MaxStudent > teacherPackage.MaxStudents)
                {
                    response.Success = false;
                    response.Message = $"MaxStudent ({requestDto.MaxStudent}) cannot exceed your package limit ({teacherPackage.MaxStudents}). Please upgrade your package.";
                    return response;
                }

                // Nếu teacher không set MaxStudent (0), tự động set = MaxStudents của package
                int courseMaxStudent = requestDto.MaxStudent > 0 ? requestDto.MaxStudent : teacherPackage.MaxStudents;

                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Img = requestDto.Img,
                    Type = requestDto.Type,
                    TeacherId = teacherId,
                    ClassCode = classCode,
                    MaxStudent = courseMaxStudent,
                    EnrollmentCount = 0,
                    IsFeatured = false
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

        public async Task<ServiceResponse<CourseResponseDto>> UpdateCourseAsync(int courseId, TeacherUpdateCourseRequestDto requestDto, int teacherId)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                var course = await _courseRepository.GetByIdAsync(courseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                // Kiểm tra quyền sở hữu
                if (course.TeacherId != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền cập nhật khóa học này";
                    return response;
                }

                // Kiểm tra package limit khi update MaxStudent
                var teacherPackage = await _teacherPackageRepository.GetInformationTeacherpackage(teacherId);
                if (teacherPackage == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy gói đăng ký đang hoạt động";
                    return response;
                }

                // Kiểm tra MaxStudent không vượt quá package limit
                if (requestDto.MaxStudent > 0 && requestDto.MaxStudent > teacherPackage.MaxStudents)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Số học sinh tối đa ({requestDto.MaxStudent}) không được vượt quá giới hạn gói ({teacherPackage.MaxStudents})";
                    return response;
                }

                // Nếu đã có students enrolled, không cho phép giảm MaxStudent xuống dưới EnrollmentCount
                if (requestDto.MaxStudent > 0 && requestDto.MaxStudent < course.EnrollmentCount)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Không thể đặt số học sinh tối đa ({requestDto.MaxStudent}) thấp hơn số lượng đã đăng ký ({course.EnrollmentCount})";
                    return response;
                }

                // Cập nhật course
                course.Title = requestDto.Title;
                course.Description = requestDto.Description;
                course.Img = requestDto.Img;
                course.Type = requestDto.Type;
                course.MaxStudent = requestDto.MaxStudent > 0 ? requestDto.MaxStudent : teacherPackage.MaxStudents;

                await _courseRepository.UpdateCourse(course);

                // Dùng Mapper thay manual
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseResponseDto.StudentCount = await _courseRepository.CountEnrolledUsers(courseId);

                response.StatusCode = 200;
                response.Data = courseResponseDto;
                response.Message = "Cập nhật khóa học thành công";

                _logger.LogInformation("Course {CourseId} updated by Teacher {TeacherId}", courseId, teacherId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi cập nhật khóa học";
                _logger.LogError(ex, "Error in UpdateCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

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
    }
}
