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
                    return response.Fail(404, "Essay không tồn tại");

                // Kiểm tra hạn nộp assessment
                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment?.DueAt != null && DateTime.UtcNow > assessment.DueAt)
                    return response.Fail(403, "Assessment đã quá hạn nộp bài");

                // Không cho nộp lại
                var existed = await _essaySubmissionRepository
                    .GetUserSubmissionForEssayAsync(userId, dto.EssayId);

                if (existed != null)
                    return response.Fail(409, "Bạn đã nộp bài essay này rồi");

                // Commit file attachment
                string? attachmentKey = null;
                if (!string.IsNullOrWhiteSpace(dto.AttachmentTempKey))
                {
                    var commit = await _minioFileStorage.CommitFileAsync(
                        dto.AttachmentTempKey, AttachmentBucket, AttachmentFolder);

                    if (!commit.Success || string.IsNullOrWhiteSpace(commit.Data))
                        return response.Fail(400, "Không thể lưu file đính kèm");

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

                return response.SuccessResult(201, "Nộp bài Essay thành công", dtoResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateSubmission failed for UserId: {UserId}, EssayId: {EssayId}. Error: {Error}", 
                    userId, dto.EssayId, ex.ToString());
                return response.Fail(500, "Lỗi hệ thống khi nộp bài Essay");
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
                    return response.Fail(404, "Submission không tồn tại");

                if (submission.UserId != userId)
                    return response.Fail(403, "Không có quyền truy cập submission này");

                var dto = _mapper.Map<EssaySubmissionDto>(submission);
                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    dto.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket, $"{AttachmentFolder}/{submission.AttachmentKey}");
                }

                return response.SuccessResult(200, "Lấy submission thành công", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMySubmissionById failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                return response.Fail(500, "Lỗi hệ thống");
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
                    return response.SuccessResult(200, "User chưa nộp bài", null);

                var dto = _mapper.Map<EssaySubmissionDto>(submission);
                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    dto.AttachmentUrl = BuildPublicUrl.BuildURL(
                        AttachmentBucket, $"{AttachmentFolder}/{submission.AttachmentKey}");
                }

                return response.SuccessResult(200, "Lấy submission thành công", dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMySubmissionForEssay failed for UserId: {UserId}, EssayId: {EssayId}. Error: {Error}", 
                    userId, essayId, ex.ToString());
                return response.Fail(500, "Lỗi hệ thống");
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
                    return response.Fail(404, "Submission không tồn tại");

                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                    return response.Fail(403, "Không có quyền cập nhật");

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
                        return response.Fail(400, "Không thể lưu file mới");

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

                return response.SuccessResult(200, "Cập nhật submission thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateSubmission failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                return response.Fail(500, "Lỗi hệ thống");
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
                    return response.Fail(404, "Submission không tồn tại");

                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                    return response.Fail(403, "Không có quyền xóa");

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    await _minioFileStorage.DeleteFileAsync(
                        $"{AttachmentFolder}/{submission.AttachmentKey}", AttachmentBucket);
                }

                return response.SuccessResult(200, "Xóa submission thành công", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteSubmission failed for SubmissionId: {SubmissionId}, UserId: {UserId}. Error: {Error}", 
                    submissionId, userId, ex.ToString());
                return response.Fail(500, "Lỗi hệ thống");
            }
        }
    }
}
