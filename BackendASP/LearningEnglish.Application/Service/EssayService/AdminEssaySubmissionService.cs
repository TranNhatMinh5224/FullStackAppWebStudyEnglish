using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.Common.Pagination;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminEssaySubmissionService : IAdminEssaySubmissionService
    {
        private readonly IEssaySubmissionRepository _essaySubmissionRepository;
        private readonly IMinioFileStorage _minioFileStorage;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminEssaySubmissionService> _logger;

        private const string AttachmentBucket = "essay-attachments";
        private const string AttachmentFolder = "real";
        private const string AvatarBucket = "avatars";
        private const string AvatarFolder = "real";

        public AdminEssaySubmissionService(
            IEssaySubmissionRepository essaySubmissionRepository,
            IMinioFileStorage minioFileStorage,
            IMapper mapper,
            ILogger<AdminEssaySubmissionService> logger)
        {
            _essaySubmissionRepository = essaySubmissionRepository;
            _minioFileStorage = minioFileStorage;
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

        public async Task<ServiceResponse<List<EssaySubmissionListDto>>> GetSubmissionsByEssayIdAsync(int essayId)
        {
            var response = new ServiceResponse<List<EssaySubmissionListDto>>();

            try
            {
                var submissions = await _essaySubmissionRepository.GetSubmissionsByEssayIdAsync(essayId);
                var submissionListDtos = _mapper.Map<List<EssaySubmissionListDto>>(submissions);

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

                var downloadResult = await _minioFileStorage.DownloadFileAsync(submission.AttachmentKey, AttachmentBucket);
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
