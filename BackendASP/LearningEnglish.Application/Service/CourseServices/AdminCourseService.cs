using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminCourseService : IAdminCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminCourseService> _logger;

        public AdminCourseService(
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<AdminCourseService> logger)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            
        }

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

                // Generate URL từ key cho tất cả courses

                response.StatusCode = 200;
                response.Data = courseDtos;
                response.Message = "Lấy danh sách khóa học thành công";

                _logger.LogInformation("Admin retrieved {Count} courses", courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in GetAllCoursesAsync");
            }

            return response;
        }

        public async Task<ServiceResponse<CourseResponseDto>> AdminCreateCourseAsync(AdminCreateCourseRequestDto requestDto)
        {
            var response = new ServiceResponse<CourseResponseDto>();

            try
            {
                // Tạo course entity
                var course = new Course
                {
                    Title = requestDto.Title,
                    Description = requestDto.Description,
                    Type = requestDto.Type,
                    Price = requestDto.Price,
                    TeacherId = null,
                    MaxStudent = requestDto.MaxStudent,
                    IsFeatured = requestDto.IsFeatured,
                    EnrollmentCount = 0
                };

                await _courseRepository.AddCourse(course);

                // Convert temp file → real file nếu có ImageTempKey
                // TODO: Implement file conversion logic here

                // Map response và generate URL từ key
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.TeacherName = "System Admin";
                courseResponseDto.LessonCount = 0;
                courseResponseDto.StudentCount = 0;

                // Generate URL từ key
                // TODO: Implement URL generation logic here

                response.StatusCode = 201;
                response.Data = courseResponseDto;
                response.Message = "Tạo khóa học thành công";

                _logger.LogInformation("Admin created course: {CourseTitle} (ID: {CourseId})", requestDto.Title, course.CourseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in AdminCreateCourseAsync");
            }

            return response;
        }

        public async Task<ServiceResponse<CourseResponseDto>> AdminUpdateCourseAsync(int courseId, AdminUpdateCourseRequestDto requestDto)
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

                // Cập nhật course
                course.Title = requestDto.Title;
                course.Description = requestDto.Description;
                course.Price = requestDto.Price;
                course.MaxStudent = requestDto.MaxStudent;
                course.IsFeatured = requestDto.IsFeatured;
                course.Type = requestDto.Type;

                // Xử lý file ảnh: xóa file cũ nếu có file mới
                // TODO: Implement file handling logic here

                await _courseRepository.UpdateCourse(course);

                // Map response và generate URL từ key
                var courseResponseDto = _mapper.Map<CourseResponseDto>(course);
                courseResponseDto.LessonCount = await _courseRepository.CountLessons(courseId);
                courseResponseDto.StudentCount = await _courseRepository.CountEnrolledUsers(courseId);
                courseResponseDto.TeacherName = course.Teacher != null 
                    ? $"{course.Teacher.FirstName} {course.Teacher.LastName}" 
                    : "System Admin";

                // Generate URL từ key
                // TODO: Implement URL generation logic here

                response.StatusCode = 200;
                response.Data = courseResponseDto;
                response.Message = "Cập nhật khóa học thành công";

                _logger.LogInformation("Admin updated course {CourseId}", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in AdminUpdateCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> DeleteCourseAsync(int courseId)
        {
            var response = new ServiceResponse<bool>();

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

                await _courseRepository.DeleteCourse(courseId);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Xóa khóa học thành công";

                _logger.LogInformation("Course {CourseId} deleted", courseId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                _logger.LogError(ex, "Error in DeleteCourseAsync for CourseId: {CourseId}", courseId);
            }

            return response;
        }
    }
}
