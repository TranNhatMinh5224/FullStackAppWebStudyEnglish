using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Essay;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.EssayService
{
    public class UserEssayService : IUserEssayService
    {
        private readonly IEssayRepository _essayRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserEssayService> _logger;

        private const string EssayAudioBucket = "essays";
        private const string EssayImageBucket = "essays";

        public UserEssayService(
            IEssayRepository essayRepository,
            IMapper mapper,
            ILogger<UserEssayService> logger)
        {
            _essayRepository = essayRepository;
            _mapper = mapper;
            _logger = logger;
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

                var essayDto = _mapper.Map<EssayDto>(essay);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                {
                    essayDto.AudioUrl = BuildPublicUrl.BuildURL(EssayAudioBucket, essay.AudioKey);
                    essayDto.AudioType = essay.AudioType;
                }

                if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                {
                    essayDto.ImageUrl = BuildPublicUrl.BuildURL(EssayImageBucket, essay.ImageKey);
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
                var essays = await _essayRepository.GetEssaysByAssessmentIdAsync(assessmentId);
                var essayDtos = new List<EssayDto>();

                foreach (var essay in essays)
                {
                    var essayDto = _mapper.Map<EssayDto>(essay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                    {
                        essayDto.AudioUrl = BuildPublicUrl.BuildURL(EssayAudioBucket, essay.AudioKey);
                        essayDto.AudioType = essay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                    {
                        essayDto.ImageUrl = BuildPublicUrl.BuildURL(EssayImageBucket, essay.ImageKey);
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
