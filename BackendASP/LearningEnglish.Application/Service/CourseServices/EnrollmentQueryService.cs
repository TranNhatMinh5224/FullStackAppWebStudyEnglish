using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class EnrollmentQueryService : IEnrollmentQueryService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentQueryService> _logger;

    
        private const string CourseImageBucket = "courses";

        public EnrollmentQueryService(
            ICourseRepository courseRepository,
            IMapper _mapper,
            ILogger<EnrollmentQueryService> logger)
        {
            _courseRepository = courseRepository;
            this._mapper = _mapper;
            _logger = logger;
        }

        // Lấy danh sách khóa học đã đăng ký của user
        public async Task<ServiceResponse<IEnumerable<CourseResponseDto>>> GetMyEnrolledCoursesAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<CourseResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetEnrolledCoursesByUser(userId);

                if (courses == null || !courses.Any())
                {
                    response.Data = Enumerable.Empty<CourseResponseDto>();
                    response.Message = "No enrolled courses found";
                    return response;
                }

                var courseDtos = _mapper.Map<IEnumerable<CourseResponseDto>>(courses).ToList();

                // Generate URL từ key cho tất cả courses
                foreach (var courseDto in courseDtos)
                {
                    if (!string.IsNullOrWhiteSpace(courseDto.ImageUrl))
                    {
                        courseDto.ImageUrl = BuildPublicUrl.BuildURL(
                            CourseImageBucket,
                            courseDto.ImageUrl
                        );
                    }
                }

                response.Success = true;
                response.Data = courseDtos;
                response.Message = $"Retrieved {courseDtos.Count()} enrolled courses";

                _logger.LogInformation("User {UserId} has {Count} enrolled courses", userId, courseDtos.Count());
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
