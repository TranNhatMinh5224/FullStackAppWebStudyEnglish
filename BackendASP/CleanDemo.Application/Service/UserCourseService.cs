using AutoMapper;
using CleanDemo.Application.Common;
using CleanDemo.Application.DTOs;
using CleanDemo.Application.Interface;
using Microsoft.Extensions.Logging;

namespace CleanDemo.Application.Service
{
    public class UserCourseService : IUserCourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ICourseRepository _userCourseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserCourseService> _logger;

        public UserCourseService(
            ICourseRepository courseRepository,
            ICourseRepository userCourseRepository,
            IMapper mapper,
            ILogger<UserCourseService> logger)
        {
            _courseRepository = courseRepository;
            _userCourseRepository = userCourseRepository;
            _mapper = mapper;
            _logger = logger;
        }
     public async Task<ServiceResponse<IEnumerable<UserCourseListResponseDto>>> GetSystemCoursesAsync(int? userId = null)
        {
            var response = new ServiceResponse<IEnumerable<UserCourseListResponseDto>>();

            try
            {
                var courses = await _courseRepository.GetSystemCourses();

                var courseDtos = _mapper.Map<IEnumerable<UserCourseListResponseDto>>(courses);

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