using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserEssaySubmissionService : IUserEssaySubmissionService
    {
        private readonly IEssaySubmissionRepository _essaySubmissionRepository;
        private readonly IEssayRepository _essayRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IMapper _mapper;
        private readonly ILogger<UserEssaySubmissionService> _logger;

        private const string AttachmentBucket = "essay-attachments";
        private const string AttachmentFolder = "real";

        public UserEssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IEssayRepository essayRepository,
            IAssessmentRepository assessmentRepository,
            INotificationRepository notificationRepository,
            IModuleProgressService moduleProgressService,
            IMinioFileStorage minioFileStorage,
            IMapper mapper,
            ILogger<UserEssaySubmissionService> logger)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _essayRepository = essayRepository;
            _assessmentRepository = assessmentRepository;
            _notificationRepository = notificationRepository;
            _moduleProgressService = moduleProgressService;
            _minioFileStorage = minioFileStorage;
            _mapper = mapper;
            _logger = logger;
        }

        // Tạo notification khi user nộp essay
        private async Task CreateEssaySubmissionNotificationAsync(int userId, string essayTitle)
        {
            try
            {
                await _notificationRepository.AddAsync(new Notification
                {
                    UserId = userId,
                    Title = " Nộp bài essay thành công",
                    Message = $"Bạn đã nộp bài essay '{essayTitle}' thành công. Giáo viên sẽ chấm điểm sớm.",
                    Type = NotificationType.AssessmentGraded,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create essay submission notification failed for UserId: {UserId}, EssayTitle: {EssayTitle}. Error: {Error}", 
                    userId, essayTitle, ex.ToString());
            }
        }

        // User nộp bài essay
        public async Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                // Kiểm tra essay tồn tại
                var essay = await _essayRepository.GetEssayByIdAsync(dto.EssayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Kiểm tra hạn nộp assessment
                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment?.DueAt != null && DateTime.UtcNow > assessment.DueAt)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Assessment đã quá hạn nộp bài";
                    return response;
                }

                // Không cho nộp lại
                var existed = await _essaySubmissionRepository
                    .GetUserSubmissionForEssayAsync(userId, dto.EssayId);

                if (existed != null)
                {
                    response.Success = false;
                    response.StatusCode = 409;
                    response.Message = "Bạn đã nộp bài essay này rồi";
                    return response;
                }

                // Commit file attachment
                string? attachmentKey = null;
                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    var commit = await _minioFileStorage.CommitFileAsync(
                        dto.AttachmentTempKey, AttachmentBucket, AttachmentFolder);

                    if (!commit.Success || string.IsNullOrWhiteSpace(commit.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu file đính kèm";
                        return response;
                    }

                    attachmentKey = commit.Data;
                }

                // Tạo submission
                var submission = new EssaySubmission
                {
                    EssayId = dto.EssayId,
                    UserId = userId,
                    TextContent = dto.TextContent,
                    AttachmentKey = attachmentKey,
                    AttachmentType = dto.AttachmentType,
                    SubmittedAt = DateTime.UtcNow,
                    Status = SubmissionStatus.Submitted
                };

                var created = await _essaySubmissionRepository.CreateSubmissionAsync(submission);

                // Hoàn thành module nếu có
                if (assessment?.ModuleId != null)
                    await _moduleProgressService.CompleteModuleAsync(userId, assessment.ModuleId);

                // Tạo notification
                await CreateEssaySubmissionNotificationAsync(userId, essay.Title);

                // Map DTO
                var dtoResult = _mapper.Map<EssaySubmissionDto>(created);
                if (!string.IsNullOrWhiteSpace(created.AttachmentKey))
                {
                    dtoResult.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket, $"{AttachmentFolder}/{created.AttachmentKey}");
                }

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Nộp bài Essay thành công";
                response.Data = dtoResult;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSubmission failed for UserId: {UserId}, EssayId: {EssayId}. Error: {Error}", 
                    userId, dto.EssayId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi nộp bài Essay";
                return response;
            }
        }

        // Lấy submission của chính user theo submissionId
        public async Task<ServiceResponse<EssaySubmissionDto>> GetMySubmissionByIdAsync(int submissionId, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                if (submission.UserId != userId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền truy cập submission này";
                    return response;
                }

                var dto = _mapper.Map<EssaySubmissionDto>(submission);
                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    dto.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket, $"{AttachmentFolder}/{submission.AttachmentKey}");
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy submission thành công";
                response.Data = dto;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMySubmissionById failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        // Lấy submission của user theo essayId
        public async Task<ServiceResponse<EssaySubmissionDto?>> GetMySubmissionForEssayAsync(int userId, int essayId)
        {
            var response = new ServiceResponse<EssaySubmissionDto?>();

            try
            {
                var submission = await _essaySubmissionRepository
                    .GetUserSubmissionForEssayAsync(userId, essayId);

                if (submission == null)
                {
                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "User chưa nộp bài";
                    response.Data = null;
                    return response;
                }

                var dto = _mapper.Map<EssaySubmissionDto>(submission);
                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    dto.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket, $"{AttachmentFolder}/{submission.AttachmentKey}");
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy submission thành công";
                response.Data = dto;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMySubmissionForEssay failed for UserId: {UserId}, EssayId: {EssayId}. Error: {Error}", 
                    userId, essayId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        // User cập nhật bài nộp
        public async Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(
            int submissionId, UpdateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền cập nhật";
                    return response;
                }

                // Xóa attachment cũ nếu yêu cầu
                if (dto.RemoveAttachment && !string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    await _minioFileStorage.DeleteFileAsync(
                        $"{AttachmentFolder}/{submission.AttachmentKey}", AttachmentBucket);
                    submission.AttachmentKey = null;
                    submission.AttachmentType = null;
                }

                // Commit attachment mới
                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    var commit = await _minioFileStorage.CommitFileAsync(
                        dto.AttachmentTempKey, AttachmentBucket, AttachmentFolder);

                    if (!commit.Success)
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu file mới";
                        return response;
                    }

                    submission.AttachmentKey = commit.Data;
                    submission.AttachmentType = dto.AttachmentType;
                }

                submission.TextContent = dto.TextContent;

                var updated = await _essaySubmissionRepository.UpdateSubmissionAsync(submission);

                var result = _mapper.Map<EssaySubmissionDto>(updated);
                if (!string.IsNullOrWhiteSpace(updated.AttachmentKey))
                {
                    result.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket, $"{AttachmentFolder}/{updated.AttachmentKey}");
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật submission thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSubmission failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }

        // User xóa bài nộp
        public async Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId, int userId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Submission không tồn tại";
                    return response;
                }

                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Không có quyền xóa";
                    return response;
                }

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    await _minioFileStorage.DeleteFileAsync(
                        $"{AttachmentFolder}/{submission.AttachmentKey}", AttachmentBucket);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa submission thành công";
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSubmission failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống";
                return response;
            }
        }
    }
}
