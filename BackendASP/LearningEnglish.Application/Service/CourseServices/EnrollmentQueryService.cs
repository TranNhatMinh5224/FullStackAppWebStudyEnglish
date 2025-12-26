using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class EnrollmentQueryService : IEnrollmentQueryService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseProgressRepository _courseProgressRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EnrollmentQueryService> _logger;


        private const string CourseImageBucket = "courses";

        public EnrollmentQueryService(
            ICourseRepository courseRepository,
            ICourseProgressRepository courseProgressRepository,
            IMapper _mapper,
            ILogger<EnrollmentQueryService> logger)
        {
            _courseRepository = courseRepository;
            _courseProgressRepository = courseProgressRepository;
            this._mapper = _mapper;
            _logger = logger;
        }

        // Lấy danh sách khóa học đã đăng ký của user với tiến độ
        public async Task<ServiceResponse<IEnumerable<EnrolledCourseWithProgressDto>>> GetMyEnrolledCoursesAsync(int userId)
        {
            var response = new ServiceResponse<IEnumerable<EnrolledCourseWithProgressDto>>();

            try
            {
                // RLS đã filter theo userId, không cần truyền userId vào repository
                var courses = await _courseRepository.GetEnrolledCoursesByUser();

                if (courses == null || !courses.Any())
                {
                    response.Data = Enumerable.Empty<EnrolledCourseWithProgressDto>();
                    response.Message = "No enrolled courses found";
                    return response;
                }

                //  Map to EnrolledCourseWithProgressDto and populate progress data
                var courseDtos = new List<EnrolledCourseWithProgressDto>();

                foreach (var course in courses)
                {
                    var courseDto = _mapper.Map<EnrolledCourseWithProgressDto>(course);

                    // Get progress information for this course
                    var courseProgress = await _courseProgressRepository.GetByUserAndCourseAsync(userId, course.CourseId);

                    if (courseProgress != null)
                    {
                        courseDto.ProgressPercentage = courseProgress.ProgressPercentage;
                        courseDto.CompletedLessons = courseProgress.CompletedLessons;
                        courseDto.TotalLessons = courseProgress.TotalLessons;
                        courseDto.IsCompleted = courseProgress.IsCompleted;
                        courseDto.EnrolledAt = courseProgress.EnrolledAt;
                        courseDto.CompletedAt = courseProgress.CompletedAt;
                    }
                    else
                    {
                        // No progress yet, set default values
                        courseDto.ProgressPercentage = 0;
                        courseDto.CompletedLessons = 0;
                        courseDto.TotalLessons = course.Lessons?.Count ?? 0;
                        courseDto.IsCompleted = false;
                        courseDto.EnrolledAt = DateTime.UtcNow;
                        courseDto.CompletedAt = null;
                    }

                    // Generate image URL
                    if (!string.IsNullOrWhiteSpace(courseDto.ImageUrl))
                    {
                        courseDto.ImageUrl = BuildPublicUrl.BuildURL(
                            CourseImageBucket,
                            courseDto.ImageUrl
                        );
                    }

                    courseDtos.Add(courseDto);
                }

                response.Success = true;
                response.Data = courseDtos;
                response.Message = $"Retrieved {courseDtos.Count} enrolled courses with progress";

                _logger.LogInformation("User {UserId} has {Count} enrolled courses", userId, courseDtos.Count);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"Error retrieving enrolled courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetMyEnrolledCoursesAsync for UserId: {UserId}", userId);
            }

            return response;
        }

        // Lấy danh sách khóa học đã đăng ký của user với tiến độ (chỉ phân trang, không filter) - RLS đã filter theo userId
        public async Task<ServiceResponse<PagedResult<EnrolledCourseWithProgressDto>>> GetMyEnrolledCoursesPagedAsync(int userId, PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<EnrolledCourseWithProgressDto>>();

            try
            {
                // RLS đã filter theo userId, không cần truyền userId vào repository
                var pagedCourses = await _courseRepository.GetEnrolledCoursesByUserPagedAsync(request);

                if (pagedCourses.Items == null || !pagedCourses.Items.Any())
                {
                    response.Data = new PagedResult<EnrolledCourseWithProgressDto>
                    {
                        Items = new List<EnrolledCourseWithProgressDto>(),
                        TotalCount = 0,
                        PageNumber = request.PageNumber,
                        PageSize = request.PageSize
                    };
                    response.Message = "No enrolled courses found";
                    return response;
                }

                // Map to EnrolledCourseWithProgressDto and populate progress data
                var courseDtos = new List<EnrolledCourseWithProgressDto>();

                foreach (var course in pagedCourses.Items)
                {
                    var courseDto = _mapper.Map<EnrolledCourseWithProgressDto>(course);

                    // Get progress information for this course
                    var courseProgress = await _courseProgressRepository.GetByUserAndCourseAsync(userId, course.CourseId);

                    if (courseProgress != null)
                    {
                        courseDto.ProgressPercentage = courseProgress.ProgressPercentage;
                        courseDto.CompletedLessons = courseProgress.CompletedLessons;
                        courseDto.TotalLessons = courseProgress.TotalLessons;
                        courseDto.IsCompleted = courseProgress.IsCompleted;
                        courseDto.EnrolledAt = courseProgress.EnrolledAt;
                        courseDto.CompletedAt = courseProgress.CompletedAt;
                    }
                    else
                    {
                        // No progress yet, set default values
                        courseDto.ProgressPercentage = 0;
                        courseDto.CompletedLessons = 0;
                        courseDto.TotalLessons = course.Lessons?.Count ?? 0;
                        courseDto.IsCompleted = false;
                        courseDto.EnrolledAt = DateTime.UtcNow;
                        courseDto.CompletedAt = null;
                    }

                    // Generate image URL
                    if (!string.IsNullOrWhiteSpace(courseDto.ImageUrl))
                    {
                        courseDto.ImageUrl = BuildPublicUrl.BuildURL(
                            CourseImageBucket,
                            courseDto.ImageUrl
                        );
                    }

                    courseDtos.Add(courseDto);
                }

                response.Success = true;
                response.Data = new PagedResult<EnrolledCourseWithProgressDto>
                {
                    Items = courseDtos,
                    TotalCount = pagedCourses.TotalCount,
                    PageNumber = pagedCourses.PageNumber,
                    PageSize = pagedCourses.PageSize
                };
                response.Message = $"Retrieved {courseDtos.Count} of {pagedCourses.TotalCount} enrolled courses";

                _logger.LogInformation("User {UserId} has {Count}/{Total} enrolled courses on page {Page}", 
                    userId, courseDtos.Count, pagedCourses.TotalCount, request.PageNumber);
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.StatusCode = 500;
                response.Message = $"Error retrieving enrolled courses: {ex.Message}";
                _logger.LogError(ex, "Error in GetMyEnrolledCoursesPagedAsync for UserId: {UserId}", userId);
            }

            return response;
        }
    }
}
