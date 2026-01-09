using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    
    public class AdminEssaySubmissionService : IAdminEssaySubmissionService
    {
        private readonly IEssaySubmissionRepository _essaySubmissionRepository;
        private readonly IEssayAttachmentService _attachmentService;
        private readonly IAvatarService _avatarService;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminEssaySubmissionService> _logger;

        public AdminEssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IEssayAttachmentService attachmentService,
            IAvatarService avatarService,
            IMapper mapper,
            ILogger<AdminEssaySubmissionService> logger)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _attachmentService = attachmentService;
            _avatarService = avatarService;
            _mapper = mapper;
            _logger = logger;
        }

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

                foreach (var submissionDto in submissionListDtos)
                {
                    var submission = submissions.FirstOrDefault(s => s.SubmissionId == submissionDto.SubmissionId);
                    if (submission?.User != null && !string.IsNullOrWhiteSpace(submission.User.AvatarKey))
                    {
                        submissionDto.UserAvatarUrl = _avatarService.BuildAvatarUrl(submission.User.AvatarKey);
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

        public async Task<ServiceResponse<EssaySubmissionDto>> GetSubmissionDetailAsync(int submissionId)
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

                var submissionDto = _mapper.Map<EssaySubmissionDto>(submission);

                if (!string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    submissionDto.AttachmentUrl = _attachmentService.BuildAttachmentUrl(submission.AttachmentKey);
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

        public async Task<ServiceResponse<bool>> DeleteSubmissionAsync(int submissionId)
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

                string? attachmentKey = submission.AttachmentKey;

                await _essaySubmissionRepository.DeleteSubmissionAsync(submissionId);

                if (!string.IsNullOrWhiteSpace(attachmentKey))
                {
                    await _attachmentService.DeleteAttachmentAsync(attachmentKey);
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

        public async Task<ServiceResponse<(Stream fileStream, string fileName, string contentType)>> DownloadSubmissionFileAsync(int submissionId)
        {
            var response = new ServiceResponse<(Stream, string, string)>();

            try
            {
                var submission = await _essaySubmissionRepository.GetSubmissionByIdAsync(submissionId);
                if (submission == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy bài nộp";
                    response.StatusCode = 404;
                    return response;
                }

                if (string.IsNullOrWhiteSpace(submission.AttachmentKey))
                {
                    response.Success = false;
                    response.Message = "Bài nộp không có file đính kèm";
                    response.StatusCode = 404;
                    return response;
                }

                var downloadResult = await _attachmentService.DownloadAttachmentAsync(submission.AttachmentKey);
                if (!downloadResult.Success || downloadResult.Data == null)
                {
                    response.Success = false;
                    response.Message = $"Không thể tải file: {downloadResult.Message}";
                    response.StatusCode = downloadResult.StatusCode;
                    return response;
                }

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
