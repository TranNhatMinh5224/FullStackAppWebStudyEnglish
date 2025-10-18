using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class EnrollmentQueryService : IEnrollmentQueryService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentQueryService> _logger;

        public EnrollmentQueryService(
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<EnrollmentQueryService> logger)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách khóa học đã đăng ký của user
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
                response.Success = true;
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
    }
}
