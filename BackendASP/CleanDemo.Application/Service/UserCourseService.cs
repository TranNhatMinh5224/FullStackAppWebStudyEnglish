using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using CleanDemo.Application.Common;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class UserCourseService : IUserCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCourseService> _logger;

        public UserCourseService(
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<UserCourseService> logger)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

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
