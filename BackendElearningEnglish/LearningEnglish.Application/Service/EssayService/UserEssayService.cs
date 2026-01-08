using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Essay;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.EssayService
{
    
    public class UserEssayService : IUserEssayService
    {
        private readonly IEssayRepository _essayRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserEssayService> _logger;
        private readonly IEssayMediaService _essayMediaService;

        public UserEssayService(
            IEssayRepository essayRepository,
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<UserEssayService> logger,
            IEssayMediaService essayMediaService)
        {
            _essayRepository = essayRepository;
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _essayMediaService = essayMediaService;
        }

        public async Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId, int userId)
        {
            var response = new ServiceResponse<EssayDto>();

            try
            {
                var essay = await _essayRepository.GetEssayByIdWithDetailsAsync(essayId);

                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Lấy Assessment với Module và Course để check enrollment
                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Lấy Module với Course để check enrollment
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
                    return response;
                }

                // Check enrollment
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
                    response.Message = "Bạn cần đăng ký khóa học để xem Essay này";
                    _logger.LogWarning("User {UserId} attempted to access essay {EssayId} without enrollment", 
                        userId, essayId);
                    return response;
                }

                var essayDto = _mapper.Map<EssayDto>(essay);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                {
                    essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(essay.AudioKey);
                    essayDto.AudioType = essay.AudioType;
                }

                if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                {
                    essayDto.ImageUrl = _essayMediaService.BuildImageUrl(essay.ImageKey);
                    essayDto.ImageType = essay.ImageType;
                }

                // TODO: Thêm logic lấy progress của user cho essay này

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thông tin Essay thành công";
                response.Data = essayDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin Essay {EssayId} cho User {UserId}", essayId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy thông tin Essay";
                return response;
            }
        }

        public async Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId, int userId)
        {
            var response = new ServiceResponse<List<EssayDto>>();

            try
            {
                // Lấy Assessment với Module và Course để check enrollment
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Lấy Module với Course để check enrollment
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
                    return response;
                }

                // Check enrollment
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
                    response.Message = "Bạn cần đăng ký khóa học để xem các Essay";
                    _logger.LogWarning("User {UserId} attempted to list essays of assessment {AssessmentId} without enrollment", 
                        userId, assessmentId);
                    return response;
                }

                var essays = await _essayRepository.GetEssaysByAssessmentIdAsync(assessmentId);
                var essayDtos = new List<EssayDto>();

                foreach (var essay in essays)
                {
                    var essayDto = _mapper.Map<EssayDto>(essay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                    {
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(essay.AudioKey);
                        essayDto.AudioType = essay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(essay.ImageKey);
                        essayDto.ImageType = essay.ImageType;
                    }

                    // TODO: Thêm logic lấy progress của user cho essay này

                    essayDtos.Add(essayDto);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Essay thành công";
                response.Data = essayDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Essay theo Assessment {AssessmentId} cho User {UserId}", assessmentId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách Essay";
                return response;
            }
        }
    }
}
