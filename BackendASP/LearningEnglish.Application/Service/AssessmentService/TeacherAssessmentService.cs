using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class TeacherAssessmentService : ITeacherAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherAssessmentService> _logger;

        public TeacherAssessmentService(
            IAssessmentRepository assessmentRepository,
            IMapper mapper,
            ILogger<TeacherAssessmentService> logger)
        {
            _assessmentRepository = assessmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<AssessmentDto>> CreateAssessmentAsync(CreateAssessmentDto dto, int teacherId)
        {
            var response = new ServiceResponse<AssessmentDto>();
            try
            {
                var moduleExists = await _assessmentRepository.ModuleExists(dto.ModuleId);
                if (!moduleExists)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
                    return response;
                }

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, dto.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền tạo Assessment cho Module này";
                    return response;
                }

                var assessment = _mapper.Map<Assessment>(dto);
                await _assessmentRepository.AddAssessment(assessment);

                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Tạo Assessment thành công";
                response.Data = assessmentDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi tạo Assessment";
                return response;
            }
        }

        public async Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleIdAsync(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<List<AssessmentDto>>();
            try
            {
                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, moduleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xem Assessments của Module này";
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

        public async Task<ServiceResponse<AssessmentDto>> GetAssessmentByIdAsync(int assessmentId, int teacherId)
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

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xem Assessment này";
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

        public async Task<ServiceResponse<AssessmentDto>> UpdateAssessmentAsync(int assessmentId, UpdateAssessmentDto dto, int teacherId)
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

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền cập nhật Assessment này";
                    return response;
                }

                _mapper.Map(dto, assessment);
                await _assessmentRepository.UpdateAssessment(assessment);

                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật Assessment thành công";
                response.Data = assessmentDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi cập nhật Assessment";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteAssessmentAsync(int assessmentId, int teacherId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    response.Data = false;
                    return response;
                }

                var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId, assessment.ModuleId);
                if (!isOwner)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xóa Assessment này";
                    response.Data = false;
                    return response;
                }

                await _assessmentRepository.DeleteAssessment(assessmentId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Assessment thành công";
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Assessment");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi xóa Assessment";
                response.Data = false;
                return response;
            }
        }
    }
}
