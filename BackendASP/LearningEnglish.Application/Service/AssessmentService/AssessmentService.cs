using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AssessmentService : IAssessmentService
    {
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AssessmentService> _logger;

        public AssessmentService(
            IAssessmentRepository assessmentRepository,
            IMapper mapper,
            ILogger<AssessmentService> logger)
        {
            _assessmentRepository = assessmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResponse<AssessmentDto>> CreateAssessment(CreateAssessmentDto dto, int? teacherId = null)
        {
            var response = new ServiceResponse<AssessmentDto>();
            try
            {
                // Kiểm tra Module tồn tại
                var moduleExists = await _assessmentRepository.ModuleExists(dto.ModuleId);
                if (!moduleExists)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
                    return response;
                }

                // Kiểm tra quyền Teacher cho Module (nếu có teacherId)
                if (teacherId.HasValue)
                {
                    var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId.Value, dto.ModuleId);
                    if (!isOwner)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền tạo Assessment cho Module này";
                        return response;
                    }
                }






                // Map DTO to Entity
                var assessment = _mapper.Map<Assessment>(dto);

                // Tạo Assessment
                await _assessmentRepository.AddAssessment(assessment);

                // Map Entity to DTO
                var assessmentDto = _mapper.Map<AssessmentDto>(assessment);

                response.Success = true;
                response.StatusCode = 200;
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
        public async Task<ServiceResponse<List<AssessmentDto>>> GetAssessmentsByModuleId(int moduleId)
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
        public async Task<ServiceResponse<AssessmentDto>> GetAssessmentById(int assessmentId)
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
        // chức năng cập nhật Assessment
        public async Task<ServiceResponse<AssessmentDto>> UpdateAssessment(int assessmentId, UpdateAssessmentDto dto)
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

                // Cập nhật các thuộc tính của Assessment. khi đó dữ liệu mới từ dto sẽ ghi đè lên dữ liệu cũ trong assessment
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
        public async Task<ServiceResponse<bool>> DeleteAssessment(int assessmentId, int? teacherId = null)
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

                // Kiểm tra quyền Teacher cho Module (nếu có teacherId)
                if (teacherId.HasValue)
                {
                    var isOwner = await _assessmentRepository.IsTeacherOwnerOfModule(teacherId.Value, assessment.ModuleId);
                    if (!isOwner)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền xóa Assessment này";
                        response.Data = false;
                        return response;
                    }
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