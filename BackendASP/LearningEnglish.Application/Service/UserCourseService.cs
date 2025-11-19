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
        private readonly IMapper _mapper;
        private readonly ILogger<UserCourseService> _logger;
        private readonly IFileStorageService _fileStorageService;

        public UserCourseService(
            ICourseRepository courseRepository,
            ICourseRepository userCourseRepository,
            IMapper mapper,
            ILogger<UserCourseService> logger,
            IFileStorageService fileStorageService)
        {
            _courseRepository = courseRepository;
            _userCourseRepository = userCourseRepository;
            _mapper = mapper;
            _logger = logger;
            _fileStorageService = fileStorageService;
        }
     public async Task<ServiceResponse<IEnumerable<UserCourseListResponseDto>>> GetSystemCoursesAsync(int? userId = null)
        {
            var response = new ServiceResponse<IEnumerable<UserCourseListResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetSystemCourses();

                var courseDtos = _mapper.Map<IEnumerable<UserCourseListResponseDto>>(courses).ToList();

                // Generate URL từ key cho mỗi course
                FileUrlHelper.SetImageUrlForUserCourseList(courses, courseDtos, _fileStorageService);

                response.StatusCode = 200;
                response.Data = courseDtos;
                response.Message = "Lấy danh sách khóa học hệ thống thành công";

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
       
    }
}
