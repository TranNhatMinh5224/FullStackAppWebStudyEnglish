using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
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
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IMapper _mapper;
        private readonly ILogger<EssaySubmissionService> _logger;
        private readonly IModuleProgressService _moduleProgressService;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IStreakService _streakService;
        private readonly INotificationRepository _notificationRepository;

        // MinIO configuration for essay attachments
        private const string AttachmentBucket = "essay-attachments";
        private const string AttachmentFolder = "real";
        
        // MinIO configuration for user avatars
        private const string AvatarBucket = "avatars";
        private const string AvatarFolder = "real";

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

        public EssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IEssayRepository essayRepository,
            IMinioFileStorage minioFileStorage,
            IMapper mapper,
            ILogger<EssaySubmissionService> logger,
            IModuleProgressService moduleProgressService,
            IAssessmentRepository assessmentRepository,
            IStreakService streakService,
            INotificationRepository notificationRepository)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _essayRepository = essayRepository;
            _minioFileStorage = minioFileStorage;
            _mapper = mapper;
            _logger = logger;
            _moduleProgressService = moduleProgressService;
            _assessmentRepository = assessmentRepository;
            _streakService = streakService;
            _notificationRepository = notificationRepository;
        }
        // Implement cho phương thức Nộp bài kiểm tra tự luận (Essay Submission)
        public async Task<ServiceResponse<EssaySubmissionDto>> CreateSubmissionAsync(CreateEssaySubmissionDto dto, int userId)
        {
            var response = new ServiceResponse<EssaySubmissionDto>();

            try
            {
                // Kiểm tra Essay có tồn tại không
                var essay = await _essayRepository.GetEssayByIdAsync(dto.EssayId);
                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Kiểm tra Assessment deadline
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

                // Kiểm tra học sinh đã nộp bài chưa
                var existingSubmission = await _essaySubmissionRepository.GetUserSubmissionForEssayAsync(userId, dto.EssayId);
                if (existingSubmission != null)
                {
                    response.Success = false;
                    response.StatusCode = 409;
                    response.Message = "Bạn đã nộp bài cho Essay này rồi";
                    return response;
                }

                // Tạo notification
                await CreateEssaySubmissionNotificationAsync(userId, essay.Title);

                // Xử lý file attachment nếu có
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

                try
                {
                    var createdSubmission = await _essaySubmissionRepository.CreateSubmissionAsync(submission);

                    // ✅ Mark module as completed after essay submission
                    var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                    if (assessment?.ModuleId != null)
                    {
                        await _moduleProgressService.CompleteModuleAsync(userId, assessment.ModuleId);
                    }

                    var submissionDto = _mapper.Map<EssaySubmissionDto>(createdSubmission);

                    // Build attachment URL if attachment exists
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
                    // Rollback: Xóa file đã commit nếu lưu DB thất bại
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
        // Implement cho phương thức Lấy thông tin submission theo ID
        public async Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionByIdAsync(int submissionId, int? userId = null)
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

                // 🔒 Validate ownership if userId is provided (Student)
                if (userId.HasValue && submission.UserId != userId.Value)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền xem bài nộp này";
                    return response;
                }

                var submissionDto = _mapper.Map<EssaySubmissionDto>(submission);

                // Build attachment URL if attachment exists
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

        // Implement cho phương thức lấy danh sách submission của một essay cụ thể với phân trang
        // RLS Policy sẽ tự động filter: Teacher chỉ thấy submissions của courses mình dạy
        // Trả về EssaySubmissionListDto (chỉ thông tin cơ bản)
        public async Task<ServiceResponse<PagedResult<EssaySubmissionListDto>>> GetSubmissionsByEssayIdPagedAsync(
            int essayId, 
            PageRequest request)
        {
            var response = new ServiceResponse<PagedResult<EssaySubmissionListDto>>();

            try
            {
                var totalCount = await _essaySubmissionRepository.GetSubmissionsCountByEssayIdAsync(essayId);
                var submissions = await _essaySubmissionRepository.GetSubmissionsByEssayIdPagedAsync(
                    essayId, 
                    request.PageNumber, 
                    request.PageSize);

                var submissionListDtos = _mapper.Map<List<EssaySubmissionListDto>>(submissions);

                // Build avatar URLs for each submission
                foreach (var submissionDto in submissionListDtos)
                {
                    var submission = submissions.FirstOrDefault(s => s.SubmissionId == submissionDto.SubmissionId);
                    if (submission?.User != null && !string.IsNullOrWhiteSpace(submission.User.AvatarKey))
                    {
                        submissionDto.UserAvatarUrl = BuildPublicUrl.BuildURL(
                            AvatarBucket,
                            $"{AvatarFolder}/{submission.User.AvatarKey}");
                    }
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = $"Lấy danh sách {submissionListDtos.Count} submission thành công";
                response.Data = new PagedResult<EssaySubmissionListDto>
                {
                    Items = submissionListDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách submission theo Essay {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách submission";
                return response;
            }
        }

        // Implement cho phương thức lấy danh sách submission của một essay cụ thể KHÔNG phân trang
        // RLS Policy sẽ tự động filter: Teacher chỉ thấy submissions của courses mình dạy
        // Trả về EssaySubmissionListDto (chỉ thông tin cơ bản)
        public async Task<ServiceResponse<List<EssaySubmissionListDto>>> GetSubmissionsByEssayIdAsync(int essayId)
        {
            var response = new ServiceResponse<List<EssaySubmissionListDto>>();

            try
            {
                var submissions = await _essaySubmissionRepository.GetSubmissionsByEssayIdAsync(essayId);
                var submissionListDtos = _mapper.Map<List<EssaySubmissionListDto>>(submissions);

                // Build avatar URLs for each submission
                foreach (var submissionDto in submissionListDtos)
                {
                    var submission = submissions.FirstOrDefault(s => s.SubmissionId == submissionDto.SubmissionId);
                    if (submission?.User != null && !string.IsNullOrWhiteSpace(submission.User.AvatarKey))
                    {
                        submissionDto.UserAvatarUrl = BuildPublicUrl.BuildURL(
                            AvatarBucket,
                            $"{AvatarFolder}/{submission.User.AvatarKey}");
                    }
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = $"Lấy danh sách {submissionListDtos.Count} submission thành công";
                response.Data = submissionListDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách submission theo Essay {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách submission";
                return response;
            }
        }

        // Implement cho phương thức lấy ra xem học sinh đã nộp bài cho Essay nào đó chưa(1 bài tự luận cụ thể)
        public async Task<ServiceResponse<EssaySubmissionDto?>> GetUserSubmissionForEssayAsync(int userId, int essayId)
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

                // Build attachment URL if attachment exists
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
        // Implement cho phương thức Cập nhật submission của học sinh
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

                // Kiểm tra quyền: chỉ user tạo submission mới có thể cập nhật
                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền cập nhật submission này";
                    return response;
                }

                // Lưu attachment key cũ để xóa nếu cần
                string? oldAttachmentKey = submission.AttachmentKey;
                string? newAttachmentKey = null;

                // Nếu RemoveAttachment = true, xóa file cũ trên MinIO và cập nhật lại submission
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

                // Xử lý file attachment mới nếu có
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

                // Cập nhật submission
                submission.TextContent = dto.TextContent;
                if (newAttachmentKey != null)
                {
                    submission.AttachmentKey = newAttachmentKey;
                    submission.AttachmentType = dto.AttachmentType;
                }

                try
                {
                    var updatedSubmission = await _essaySubmissionRepository.UpdateSubmissionAsync(submission);

                    // Xóa file cũ nếu có file mới và có file cũ
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

                    // Build attachment URL if attachment exists
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
                    // Rollback: Xóa file mới nếu lưu DB thất bại
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

                // Kiểm tra quyền: chỉ user tạo submission mới có thể xóa
                if (!await _essaySubmissionRepository.IsUserOwnerOfSubmissionAsync(userId, submissionId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền xóa submission này";
                    return response;
                }

                // Lưu attachment key để xóa file sau khi xóa DB thành công
                string? attachmentKey = submission.AttachmentKey;

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                // Xóa file attachment nếu có
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

        // Teacher - Download file attachment of submission
        public async Task<ServiceResponse<(Stream fileStream, string fileName, string contentType)>> DownloadSubmissionFileAsync(int submissionId)
        {
            var response = new ServiceResponse<(Stream, string, string)>();

            try
            {
                // Lấy thông tin submission
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy bài nộp";
                    response.StatusCode = 404;
                    return response;
                }

                // Kiểm tra có file đính kèm không
                if (string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    response.Success = false;
                    response.Message = "Bài nộp không có file đính kèm";
                    response.StatusCode = 404;
                    return response;
                }

                // Download file từ MinIO
                var downloadResult = await _minioFileStorage.DownloadFileAsync(submission.AttachmentKey, AttachmentBucket);
                if (!downloadResult.Success || downloadResult.Data == null)
                {
                    response.Success = false;
                    response.Message = $"Không thể tải file: {downloadResult.Message}";
                    response.StatusCode = downloadResult.StatusCode;
                    return response;
                }

                // Lấy tên file từ AttachmentKey (lấy phần cuối của path)
                var fileName = Path.GetFileName(submission.AttachmentKey);
                var contentType = submission.AttachmentType ?? "application/octet-stream";

                response.Data = (downloadResult.Data, fileName, contentType);
                response.Success = true;
                response.Message = "Tải file thành công";
                response.StatusCode = 200;

                _logger.LogInformation("Downloaded submission file. SubmissionId={SubmissionId}, FileName={FileName}", 
                    submissionId, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải file submission. SubmissionId={SubmissionId}", submissionId);
                response.Success = false;
                response.Message = $"Lỗi: {ex.Message}";
                response.StatusCode = 500;
            }

            return response;
        }
    }
}