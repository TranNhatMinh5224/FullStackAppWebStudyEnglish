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
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IMapper _mapper;
        private readonly ILogger<UserEssaySubmissionService> _logger;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly INotificationRepository _notificationRepository;

        private const string AttachmentBucket = "essay-attachments";
        private const string AttachmentFolder = "real";

        public UserEssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IEssayRepository essayRepository,
            IMinioFileStorage minioFileStorage,
            IMapper mapper,
            ILogger<UserEssaySubmissionService> logger,
            IModuleProgressService moduleProgressService,
            IAssessmentRepository assessmentRepository,
            INotificationRepository notificationRepository)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _essayRepository = essayRepository;
            _minioFileStorage = minioFileStorage;
            _mapper = mapper;
            _logger = logger;
            _moduleProgressService = moduleProgressService;
            _assessmentRepository = assessmentRepository;
            _notificationRepository = notificationRepository;
        }

        private async Task CreateEssaySubmissionNotificationAsync(int userId, string essayTitle)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Title = "✅ Nộp bài essay thành công",
                    Message = $"Bạn đã nộp bài essay '{essayTitle}' thành công. Giáo viên sẽ chấm điểm sớm nhất có thể.",
                    Type = NotificationType.AssessmentGraded,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _notificationRepository.AddAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create essay submission notification");
            }
        }

        public async Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                var essay = await _essayRepository.GetEssayByIdAsync(dto.EssayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                var essayAssessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (essayAssessment != null && essayAssessment.DueAt.HasValue)
                {
                    if (DateTime.UtcNow > essayAssessment.DueAt.Value)
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Assessment đã quá hạn nộp bài";
                        return response;
                    }
                }

                var existingSubmission = await _essaySubmissionRepository.GetUserSubmissionForEssayAsync(userId, dto.EssayId);
                if (existingSubmission != null)
                {
                    response.Success = false;
                    response.StatusCode = 409;
                    response.Message = "Bạn đã nộp bài cho Essay này rồi";
                    return response;
                }

                await CreateEssaySubmissionNotificationAsync(userId, essay.Title);

                string? attachmentKey = null;
                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        dto.AttachmentTempKey,
                        AttachmentBucket,
                        AttachmentFolder);

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        _logger.LogError("Không thể commit attachment file từ temp sang permanent storage");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu file đính kèm. Vui lòng thử lại.";
                        return response;
                    }

                    attachmentKey = commitResult.Data;
                }

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

                try
                {
                    var createdSubmission = await _essaySubmissionRepository.CreateSubmissionAsync(submission);

                    var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                    if (assessment?.ModuleId != null)
                    {
                        await _moduleProgressService.CompleteModuleAsync(userId, assessment.ModuleId);
                    }

                    var submissionDto = _mapper.Map<EssaySubmissionDto>(createdSubmission);

                    if (!string.IsNullOrWhiteSpace(createdSubmission.AttachmentKey))
                    {
                        submissionDto.AttachmentUrl = BuildPublicUrl.BuildURL(
                            AttachmentBucket,
                            $"{AttachmentFolder}/{createdSubmission.AttachmentKey}");
                    }

                    response.Success = true;
                    response.StatusCode = 201;
                    response.Message = "Nộp bài Essay thành công";
                    response.Data = submissionDto;

                    return response;
                }
                catch (Exception dbEx)
                {
                    if (!string.IsNullOrWhiteSpace(attachmentKey))
                    {
                        try
                        {
                            var deleteResult = await _minioFileStorage.DeleteFileAsync($"{AttachmentFolder}/{attachmentKey}", AttachmentBucket);
                            if (deleteResult.Success)
                            {
                                _logger.LogInformation("Đã rollback attachment file sau khi lưu DB thất bại");
                            }
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Lỗi khi rollback attachment file");
                        }
                    }
                    _logger.LogError(dbEx, "Lỗi DB khi tạo submission");
                    throw;
                }
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
                    response.Message = "Bạn không có quyền xem bài nộp này";
                    return response;
                }

                var submissionDto = _mapper.Map<EssaySubmissionDto>(submission);

                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    submissionDto.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket,
                        $"{AttachmentFolder}/{submission.AttachmentKey}");
                }

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

        public async Task<ServiceResponse<EssaySubmissionDto?>> GetMySubmissionForEssayAsync(int userId, int essayId)
        {
            var response = new ServiceResponse<EssaySubmissionDto?>();

            try
            {
                var submission = await _essaySubmissionRepository.GetUserSubmissionForEssayAsync(userId, essayId);

                if (submission == null)
                {
                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "User chưa nộp bài cho Essay này";
                    response.Data = null;
                    return response;
                }

                var submissionDto = _mapper.Map<EssaySubmissionDto>(submission);

                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    submissionDto.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket,
                        $"{AttachmentFolder}/{submission.AttachmentKey}");
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy submission thành công";
                response.Data = submissionDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy submission của user {UserId} cho Essay {EssayId}", userId, essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy submission";
                return response;
            }
        }

        public async Task<ServiceResponse<EssaySubmissionDto>> UpdateSubmissionAsync(int submissionId, UpdateEssaySubmissionDto dto, int userId)
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
                    response.Message = "Bạn không có quyền cập nhật submission này";
                    return response;
                }

                string? oldAttachmentKey = submission.AttachmentKey;
                string? newAttachmentKey = null;

                if (dto.RemoveAttachment)
                {
                    if (!string.IsNullOrWhiteSpace(oldAttachmentKey))
                    {
                        try
                        {
                            await _minioFileStorage.DeleteFileAsync($"{AttachmentFolder}/{oldAttachmentKey}", AttachmentBucket);
                            _logger.LogInformation("Đã xóa attachment cũ theo yêu cầu RemoveAttachment");
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Lỗi khi xóa attachment cũ (RemoveAttachment)");
                        }
                    }
                    submission.AttachmentKey = null;
                    submission.AttachmentType = null;
                }

                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    var commitResult = await _minioFileStorage.CommitFileAsync(
                        dto.AttachmentTempKey,
                        AttachmentBucket,
                        AttachmentFolder);

                    if (!commitResult.Success || string.IsNullOrWhiteSpace(commitResult.Data))
                    {
                        _logger.LogError("Không thể commit attachment file từ temp sang permanent storage");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu file đính kèm. Vui lòng thử lại.";
                        return response;
                    }

                    newAttachmentKey = commitResult.Data;
                }

                submission.TextContent = dto.TextContent;
                if (newAttachmentKey != null)
                {
                    submission.AttachmentKey = newAttachmentKey;
                    submission.AttachmentType = dto.AttachmentType;
                }

                try
                {
                    var updatedSubmission = await _essaySubmissionRepository.UpdateSubmissionAsync(submission);

                    if (newAttachmentKey != null && !string.IsNullOrWhiteSpace(oldAttachmentKey))
                    {
                        try
                        {
                            await _minioFileStorage.DeleteFileAsync($"{AttachmentFolder}/{oldAttachmentKey}", AttachmentBucket);
                            _logger.LogInformation("Đã xóa attachment cũ sau khi cập nhật thành công");
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Lỗi khi xóa attachment cũ (không ảnh hưởng đến update)");
                        }
                    }

                    var submissionDto = _mapper.Map<EssaySubmissionDto>(updatedSubmission);

                    if (!string.IsNullOrWhiteSpace(updatedSubmission.AttachmentKey))
                    {
                        submissionDto.AttachmentUrl = BuildPublicUrl.BuildURL(
                            AttachmentBucket,
                            $"{AttachmentFolder}/{updatedSubmission.AttachmentKey}");
                    }

                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Cập nhật submission thành công";
                    response.Data = submissionDto;

                    return response;
                }
                catch (Exception dbEx)
                {
                    if (newAttachmentKey != null)
                    {
                        try
                        {
                            await _minioFileStorage.DeleteFileAsync($"{AttachmentFolder}/{newAttachmentKey}", AttachmentBucket);
                            _logger.LogInformation("Đã rollback attachment file mới sau khi update DB thất bại");
                        }
                        catch (Exception deleteEx)
                        {
                            _logger.LogError(deleteEx, "Lỗi khi rollback attachment file mới");
                        }
                    }
                    _logger.LogError(dbEx, "Lỗi DB khi update submission");
                    throw;
                }
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
                    response.Message = "Bạn không có quyền xóa submission này";
                    return response;
                }

                string? attachmentKey = submission.AttachmentKey;

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                if (!string.IsNullOrWhiteSpace(attachmentKey))
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync($"{AttachmentFolder}/{attachmentKey}", AttachmentBucket);
                        _logger.LogInformation("Đã xóa attachment file sau khi xóa submission thành công");
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx, "Lỗi khi xóa attachment file (không ảnh hưởng đến xóa submission)");
                    }
                }

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
