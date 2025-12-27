using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserAssessmentService : IUserAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserAssessmentService> _logger;

        public UserAssessmentService(
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<UserAssessmentService> logger)
        {
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<AssessmentDto>>();
            try
            {
                // Lấy module để check course
                var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check enrollment: user phải đăng ký course mới được xem assessment
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem assessment";
                    _logger.LogWarning("User {UserId} attempted to list assessments of module {ModuleId} without enrollment in course {CourseId}", 
                        userId, moduleId, courseId.Value);
                    return response;
                }

                var assessments = await _assessmentRepository.GetAssessmentsByModuleId(moduleId);
                var assessmentDtos = _mapper.Map<List<AssessmentDto>>(assessments);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Assessments thành công";
                response.Data = assessmentDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Assessments");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy danh sách Assessments";
                return response;
            }
        }

        public async Task<ServiceResponse<AssessmentDto>> GetAssessmentByIdAsync(int assessmentId, int userId)
        {
            var response = new ServiceResponse<AssessmentDto>();
            try
            {
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Check enrollment: user phải đăng ký course mới được xem assessment
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem assessment này";
                    _logger.LogWarning("User {UserId} attempted to access assessment {AssessmentId} without enrollment in course {CourseId}", 
                        userId, assessmentId, courseId.Value);
                    return response;
                }

                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy Assessment thành công";
                response.Data = assessmentDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy Assessment";
                return response;
            }
        }
    }
}
