using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class EssayService : IEssayService
    {
        private readonly IEssayRepository _essayRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<EssayService> _logger;

        public EssayService(IEssayRepository essayRepository, IMapper mapper, ILogger<EssayService> logger)
        {
            _essayRepository = essayRepository;
            _mapper = mapper;
            _logger = logger;
        }
        // Implement cho phương thức Thêm bài kiểm tra tự luận (Essay)
        public async Task<ServiceResponse<EssayDto>> CreateEssayAsync(CreateEssayDto dto, int? teacherId = null)
        {
            var response = new ServiceResponse<EssayDto>();

            try
            {
                // Kiểm tra Assessment có tồn tại không
                if (!await _essayRepository.AssessmentExistsAsync(dto.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Assessment không tồn tại";
                    return response;
                }

                // Kiểm tra quyền Teacher nếu có
                if (teacherId.HasValue)
                {
                    if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, dto.AssessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền tạo Essay cho Assessment này";
                        return response;
                    }
                }

                // Map DTO to Entity
                var essay = _mapper.Map<Essay>(dto);
                essay.Type = AssessmentType.Essay;

                // Tạo Essay
                var createdEssay = await _essayRepository.CreateEssayAsync(essay);

                // Map Entity to DTO
                var essayDto = _mapper.Map<EssayDto>(createdEssay);

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Tạo Essay thành công";
                response.Data = essayDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo Essay");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi tạo Essay";
                return response;
            }
        }
        // Implement cho phương thức Lấy thông tin bài kiểm tra tự luận (Essay) theo ID
        public async Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId)
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

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thông tin Essay thành công";
                response.Data = essayDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy thông tin Essay";
                return response;
            }
        }
        // Implement cho phương thức Lấy danh sách bài kiểm tra tự luận (Essay) theo Assessment ID
        public async Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId)
        {
            var response = new ServiceResponse<List<EssayDto>>();

            try
            {
                var essays = await _essayRepository.GetEssaysByAssessmentIdAsync(assessmentId);
                var essayDtos = _mapper.Map<List<EssayDto>>(essays);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Essay thành công";
                response.Data = essayDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Essay theo Assessment ID {AssessmentId}", assessmentId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách Essay";
                return response;
            }
        }
        // Implement cho phương thức Cập nhật bài kiểm tra tự luận (Essay)
        public async Task<ServiceResponse<EssayDto>> UpdateEssayAsync(int essayId, UpdateEssayDto dto, int? teacherId = null)
        {
            var response = new ServiceResponse<EssayDto>();

            try
            {
                var existingEssay = await _essayRepository.GetEssayByIdWithDetailsAsync(essayId);
                
                if (existingEssay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Kiểm tra quyền Teacher nếu có
                if (teacherId.HasValue)
                {
                    if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, existingEssay.AssessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền cập nhật Essay này";
                        return response;
                    }
                }

                // Cập nhật thông tin
                existingEssay.Title = dto.Title;
                existingEssay.Description = dto.Description;

                var updatedEssay = await _essayRepository.UpdateEssayAsync(existingEssay);
                var essayDto = _mapper.Map<EssayDto>(updatedEssay);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật Essay thành công";
                response.Data = essayDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi cập nhật Essay";
                return response;
            }
        }
        // Implement cho phương thức DeleteEssay (hủy nộp bài)
        public async Task<ServiceResponse<bool>> DeleteEssayAsync(int essayId, int? teacherId = null)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var existingEssay = await _essayRepository.GetEssayByIdWithDetailsAsync(essayId);
                
                if (existingEssay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }
  
                await _essayRepository.DeleteEssayAsync(essayId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Essay thành công";
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi xóa Essay";
                return response;
            }
        }
    }
}