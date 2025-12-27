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
        private readonly IMapper _mapper;
        private readonly ILogger<UserAssessmentService> _logger;

        public UserAssessmentService(
            IAssessmentRepository assessmentRepository,
            IMapper mapper,
            ILogger<UserAssessmentService> logger)
        {
            _assessmentRepository = assessmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<AssessmentDto>>();
            try
            {
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
