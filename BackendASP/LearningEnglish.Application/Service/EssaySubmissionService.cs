using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class EssaySubmissionService : IEssaySubmissionService
    {
        private readonly IEssaySubmissionRepository _essaySubmissionRepository;
        private readonly IEssayRepository _essayRepository; // Cần để check teacher permission
        private readonly IMapper _mapper;
        private readonly ILogger<EssaySubmissionService> _logger;

        public EssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IEssayRepository essayRepository,
            IMapper mapper, 
            ILogger<EssaySubmissionService> logger)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _essayRepository = essayRepository;
            _mapper = mapper;
            _logger = logger;
        }
        // Implement cho phương thức Nộp bài kiểm tra tự luận (Essay Submission)
        public async Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                // Kiểm tra Assessment có tồn tại không
                if (!await _essaySubmissionRepository.AssessmentExistsAsync(dto.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Assessment không tồn tại";
                    return response;
                }

                // Kiểm tra học sinh đã nộp bài chưa
                var existingSubmission = await _essaySubmissionRepository.GetUserSubmissionForEssayAsync(userId, dto.AssessmentId);
                if (existingSubmission != null)
                {
                    response.Success = false;
                    response.StatusCode = 409;
                    response.Message = "Bạn đã nộp bài cho Essay này rồi";
                    return response;
                }

                // Tạo submission
                var submission = new EssaySubmission
                {
                    AssessmentId = dto.AssessmentId,
                    UserId = userId,
                    TextContent = dto.TextContent,
                    SubmittedAt = DateTime.UtcNow,
                    Status = StatusSubmission.Submitted
                };

                var createdSubmission = await _essaySubmissionRepository.CreateSubmissionAsync(submission);
                var submissionDto = _mapper.Map<EssaySubmissionDto>(createdSubmission);

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Nộp bài Essay thành công";
                response.Data = submissionDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi nộp bài Essay");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi nộp bài Essay";
                return response;
            }
        }
        // Implement cho phương thức Lấy thông tin submission theo ID
        public async Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionByIdAsync(int submissionId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdWithDetailsAsync(submissionId);
                
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                var submissionDto = _mapper.Map<EssaySubmissionDto>(submission);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thông tin submission thành công";
                response.Data = submissionDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin submission");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy thông tin submission";
                return response;
            }
        }
        // Implement cho phương thức Lấy danh sách submission của user theo User ID dành cho Admin và Teacher
        public async Task<ServiceResponse<List<EssaySubmissionDto>>> GetSubmissionsByUserIdAsync(int userId)
        {
            var response = new ServiceResponse<List<EssaySubmissionDto>>();

            try
            {
                var submissions = await _essaySubmissionRepository.GetSubmissionsByUserIdAsync(userId);
                var submissionDtos = _mapper.Map<List<EssaySubmissionDto>>(submissions);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách submission của user thành công";
                response.Data = submissionDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách submission của user {UserId}", userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách submission";
                return response;
            }
        }
        // Implement cho phương thức Lấy danh sách submission theo Assessment ID dành cho Admin và Teacher
        public async Task<ServiceResponse<List<EssaySubmissionDto>>> GetSubmissionsByAssessmentIdAsync(int assessmentId, int? teacherId = null)
        {
            var response = new ServiceResponse<List<EssaySubmissionDto>>();

            try
            {
                // Nếu có teacherId, kiểm tra quyền
                if (teacherId.HasValue)
                {
                    if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, assessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn không có quyền xem submission của Assessment này";
                        return response;
                    }
                }

                var submissions = await _essaySubmissionRepository.GetSubmissionsByAssessmentIdAsync(assessmentId);
                var submissionDtos = _mapper.Map<List<EssaySubmissionDto>>(submissions);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách submission theo Assessment thành công";
                response.Data = submissionDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách submission theo Assessment {AssessmentId}", assessmentId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách submission";
                return response;
            }
        }
        // Implement cho phương thức lấy ra xem học sinh đã nộp bài cho Essay nào đó chưa(1 bài tự luận cụ thể)
        public async Task<ServiceResponse<EssaySubmissionDto?>> GetUserSubmissionForEssayAsync(int userId, int assessmentId)
        {
            var response = new ServiceResponse<EssaySubmissionDto?>();

            try
            {
                var submission = await _essaySubmissionRepository.GetUserSubmissionForEssayAsync(userId, assessmentId);
                var submissionDto = submission != null ? _mapper.Map<EssaySubmissionDto>(submission) : null;

                response.Success = true;
                response.StatusCode = 200;
                response.Message = submission != null ? "Lấy submission của user thành công" : "User chưa nộp bài cho Essay này";
                response.Data = submissionDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy submission của user {UserId} cho Assessment {AssessmentId}", userId, assessmentId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy submission";
                return response;
            }
        }
        // Implement cho phương thức Cập nhật submission của học sinh
        public async Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(int submissionId, UpdateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdWithDetailsAsync(submissionId);
                
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                // Kiểm tra quyền: chỉ user tạo submission mới có thể cập nhật
                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền cập nhật submission này";
                    return response;
                }

                // Cập nhật submission
                submission.TextContent = dto.TextContent;

                var updatedSubmission = await _essaySubmissionRepository.UpdateSubmissionAsync(submission);
                var submissionDto = _mapper.Map<EssaySubmissionDto>(updatedSubmission);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật submission thành công";
                response.Data = submissionDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật submission {SubmissionId}", submissionId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi cập nhật submission";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdWithDetailsAsync(submissionId);
                
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                // Kiểm tra quyền: chỉ user tạo submission mới có thể xóa
                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền xóa submission này";
                    return response;
                }

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa submission thành công";
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa submission {SubmissionId}", submissionId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi xóa submission";
                return response;
            }
        }
    }
}