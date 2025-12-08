using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserCourseService : IUserCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseRepository _userCourseRepository;
        private readonly ICourseProgressRepository _courseProgressRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCourseService> _logger;

        
        private const string CourseImageBucket = "courses";

        public UserCourseService(
            ICourseRepository courseRepository,
            ICourseRepository userCourseRepository,
            ICourseProgressRepository courseProgressRepository,
            IMapper mapper,
            ILogger<UserCourseService> logger)
        {
            _courseRepository = courseRepository;
            _userCourseRepository = userCourseRepository;
            _courseProgressRepository = courseProgressRepository;
            _mapper = mapper;
            _logger = logger;
        }
        // GET /api/user/courses/system-courses
        public async Task<ServiceResponse<IEnumerable<SystemCoursesListResponseDto>>> GetSystemCoursesAsync(int? userId = null)
        {
            var response = new ServiceResponse<IEnumerable<SystemCoursesListResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetSystemCourses();

                var courseDtos = _mapper.Map<IEnumerable<SystemCoursesListResponseDto>>(courses).ToList();

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
                    
                    // Check enrollment status nếu user đã login
                    if (userId.HasValue)
                    {
                        courseDto.IsEnrolled = await _courseRepository.IsUserEnrolled(courseDto.CourseId, userId.Value);
                    }
                    else
                    {
                        courseDto.IsEnrolled = false;
                    }
                }

                response.StatusCode = 200;
                response.Data = courseDtos;
                response.Message = "Lấy danh sách khóa học hệ thống thành công";
                response.Success = true;

                _logger.LogInformation("User retrieved {Count} system courses", courseDtos.Count());
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy danh sách khóa học hệ thống: {ex.Message}";
                _logger.LogError(ex, "Error in GetSystemCoursesAsync");
            }

            return response;
        }

        // GET /api/user/courses/{courseId}
        public async Task<ServiceResponse<CourseDetailWithEnrollmentDto>> GetCourseByIdAsync(int courseId, int? userId = null)
        {
            var response = new ServiceResponse<CourseDetailWithEnrollmentDto>();

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

                var courseDto = _mapper.Map<CourseDetailWithEnrollmentDto>(course);

                // Generate URL từ key
                if (!string.IsNullOrWhiteSpace(courseDto.ImageUrl))
                {
                    courseDto.ImageUrl = BuildPublicUrl.BuildURL(CourseImageBucket, courseDto.ImageUrl);
                }

                // Check enrollment status nếu user đã login
                if (userId.HasValue)
                {
                    courseDto.IsEnrolled = await _courseRepository.IsUserEnrolled(courseId, userId.Value);
                    
                    //  Add progress info if enrolled
                    if (courseDto.IsEnrolled)
                    {
                        var courseProgress = await _courseProgressRepository.GetByUserAndCourseAsync(userId.Value, courseId);
                        if (courseProgress != null)
                        {
                            courseDto.ProgressPercentage = courseProgress.ProgressPercentage;
                            courseDto.CompletedLessons = courseProgress.CompletedLessons;
                            courseDto.IsCompleted = courseProgress.IsCompleted;
                            courseDto.EnrolledAt = courseProgress.EnrolledAt;
                            courseDto.CompletedAt = courseProgress.CompletedAt;
                        }
                    }
                }
                else
                {
                    courseDto.IsEnrolled = false;
                }

                response.StatusCode = 200;
                response.Data = courseDto;
                response.Message = "Lấy thông tin khóa học thành công";
                response.Success = true;

                _logger.LogInformation("Retrieved course {CourseId} details, userId: {UserId}", courseId, userId);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Lỗi khi lấy thông tin khóa học: {ex.Message}";
                _logger.LogError(ex, "Error in GetCourseByIdAsync for course {CourseId}", courseId);
            }

            return response;
        }
       
    }
}
