using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class CourseQueryService : ICourseQueryService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CourseQueryService> _logger;

        public CourseQueryService(
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<CourseQueryService> logger)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

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
    }
}
