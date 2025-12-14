using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
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
        private readonly IMinioFileStorage _minioFileStorage;

        // Bucket and folder configuration
        private const string EssayAudioBucket = "essays";
        private const string EssayAudioFolder = "audios";
        private const string EssayImageBucket = "essays";
        private const string EssayImageFolder = "images";

        public EssayService(
            IEssayRepository essayRepository,
            IMapper mapper,
            ILogger<EssayService> logger,
            IMinioFileStorage minioFileStorage)
        {
            _essayRepository = essayRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
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
                var essay = new Essay
                {
                    AssessmentId = dto.AssessmentId,
                    Title = dto.Title,
                    Description = dto.Description,
                    Type = AssessmentType.Essay
                };

                string? committedAudioKey = null;
                string? committedImageKey = null;

                try
                {
                    // Commit Audio file nếu có
                    if (!string.IsNullOrWhiteSpace(dto.AudioTempKey))
                    {
                        var audioCommitResult = await _minioFileStorage.CommitFileAsync(
                            dto.AudioTempKey,
                            EssayAudioBucket,
                            EssayAudioFolder
                        );

                        if (!audioCommitResult.Success || string.IsNullOrWhiteSpace(audioCommitResult.Data))
                        {
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file audio. Vui lòng thử lại.";
                            return response;
                        }

                        committedAudioKey = audioCommitResult.Data;
                        essay.AudioKey = committedAudioKey;
                        essay.AudioType = dto.AudioType;
                    }

                    // Commit Image file nếu có
                    if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                    {
                        var imageCommitResult = await _minioFileStorage.CommitFileAsync(
                            dto.ImageTempKey,
                            EssayImageBucket,
                            EssayImageFolder
                        );

                        if (!imageCommitResult.Success || string.IsNullOrWhiteSpace(imageCommitResult.Data))
                        {
                            // Rollback audio nếu đã commit
                            if (committedAudioKey != null)
                            {
                                await _minioFileStorage.DeleteFileAsync(committedAudioKey, EssayAudioBucket);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file hình ảnh. Vui lòng thử lại.";
                            return response;
                        }

                        committedImageKey = imageCommitResult.Data;
                        essay.ImageKey = committedImageKey;
                        essay.ImageType = dto.ImageType;
                    }

                    // Tạo Essay trong database
                    var createdEssay = await _essayRepository.CreateEssayAsync(essay);

                    // Map Entity to DTO
                    var essayDto = _mapper.Map<EssayDto>(createdEssay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(createdEssay.AudioKey))
                    {
                        essayDto.AudioUrl = BuildPublicUrl.BuildURL(EssayAudioBucket, createdEssay.AudioKey);
                        essayDto.AudioType = createdEssay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(createdEssay.ImageKey))
                    {
                        essayDto.ImageUrl = BuildPublicUrl.BuildURL(EssayImageBucket, createdEssay.ImageKey);
                        essayDto.ImageType = createdEssay.ImageType;
                    }

                    response.Success = true;
                    response.StatusCode = 201;
                    response.Message = "Tạo Essay thành công";
                    response.Data = essayDto;

                    _logger.LogInformation("Created Essay {EssayId} with audio: {HasAudio}, image: {HasImage}",
                        createdEssay.EssayId,
                        !string.IsNullOrWhiteSpace(committedAudioKey),
                        !string.IsNullOrWhiteSpace(committedImageKey));

                    return response;
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating Essay");

                    // Rollback files
                    if (committedAudioKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedAudioKey, EssayAudioBucket);
                    }
                    if (committedImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, EssayImageBucket);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo Essay";
                    return response;
                }
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

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                {
                    essayDto.AudioUrl = BuildPublicUrl.BuildURL(EssayAudioBucket, essay.AudioKey);
                    essayDto.AudioType = essay.AudioType;
                }

                if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                {
                    essayDto.ImageUrl = BuildPublicUrl.BuildURL(EssayImageBucket, essay.ImageKey);
                    essayDto.ImageType = essay.ImageType;
                }

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
                var essayDtos = new List<EssayDto>();

                foreach (var essay in essays)
                {
                    var essayDto = _mapper.Map<EssayDto>(essay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                    {
                        essayDto.AudioUrl = BuildPublicUrl.BuildURL(EssayAudioBucket, essay.AudioKey);
                        essayDto.AudioType = essay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                    {
                        essayDto.ImageUrl = BuildPublicUrl.BuildURL(EssayImageBucket, essay.ImageKey);
                        essayDto.ImageType = essay.ImageType;
                    }

                    essayDtos.Add(essayDto);
                }

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

                // Cập nhật thông tin cơ bản (nullable support)
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    existingEssay.Title = dto.Title;

                if (dto.Description != null)
                    existingEssay.Description = dto.Description;

                string? newAudioKey = null;
                string? newImageKey = null;
                string? oldAudioKey = !string.IsNullOrWhiteSpace(existingEssay.AudioKey) ? existingEssay.AudioKey : null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(existingEssay.ImageKey) ? existingEssay.ImageKey : null;

                try
                {
                    // Xử lý Audio file mới
                    if (!string.IsNullOrWhiteSpace(dto.AudioTempKey))
                    {
                        var audioCommitResult = await _minioFileStorage.CommitFileAsync(
                            dto.AudioTempKey,
                            EssayAudioBucket,
                            EssayAudioFolder
                        );

                        if (!audioCommitResult.Success || string.IsNullOrWhiteSpace(audioCommitResult.Data))
                        {
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file audio mới. Vui lòng thử lại.";
                            return response;
                        }

                        newAudioKey = audioCommitResult.Data;
                        existingEssay.AudioKey = newAudioKey;
                        existingEssay.AudioType = dto.AudioType;
                    }

                    // Xử lý Image file mới
                    if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                    {
                        var imageCommitResult = await _minioFileStorage.CommitFileAsync(
                            dto.ImageTempKey,
                            EssayImageBucket,
                            EssayImageFolder
                        );

                        if (!imageCommitResult.Success || string.IsNullOrWhiteSpace(imageCommitResult.Data))
                        {
                            // Rollback audio nếu đã commit
                            if (newAudioKey != null && newAudioKey != oldAudioKey)
                            {
                                await _minioFileStorage.DeleteFileAsync(newAudioKey, EssayAudioBucket);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file hình ảnh mới. Vui lòng thử lại.";
                            return response;
                        }

                        newImageKey = imageCommitResult.Data;
                        existingEssay.ImageKey = newImageKey;
                        existingEssay.ImageType = dto.ImageType;
                    }

                    // Update database
                    var updatedEssay = await _essayRepository.UpdateEssayAsync(existingEssay);

                    // Xóa file cũ sau khi update thành công
                    if (newAudioKey != null && oldAudioKey != null && newAudioKey != oldAudioKey)
                    {
                        await _minioFileStorage.DeleteFileAsync(oldAudioKey, EssayAudioBucket);
                    }

                    if (newImageKey != null && oldImageKey != null && newImageKey != oldImageKey)
                    {
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, EssayImageBucket);
                    }

                    // Map to DTO và generate URLs
                    var essayDto = _mapper.Map<EssayDto>(updatedEssay);

                    if (!string.IsNullOrWhiteSpace(updatedEssay.AudioKey))
                    {
                        essayDto.AudioUrl = BuildPublicUrl.BuildURL(EssayAudioBucket, updatedEssay.AudioKey);
                        essayDto.AudioType = updatedEssay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(updatedEssay.ImageKey))
                    {
                        essayDto.ImageUrl = BuildPublicUrl.BuildURL(EssayImageBucket, updatedEssay.ImageKey);
                        essayDto.ImageType = updatedEssay.ImageType;
                    }

                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Cập nhật Essay thành công";
                    response.Data = essayDto;

                    _logger.LogInformation("Updated Essay {EssayId}", essayId);

                    return response;
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating Essay");

                    // Rollback new files
                    if (newAudioKey != null && newAudioKey != oldAudioKey)
                    {
                        await _minioFileStorage.DeleteFileAsync(newAudioKey, EssayAudioBucket);
                    }
                    if (newImageKey != null && newImageKey != oldImageKey)
                    {
                        await _minioFileStorage.DeleteFileAsync(newImageKey, EssayImageBucket);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật Essay";
                    return response;
                }
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
        // Implement cho phương thức DeleteEssay (xóa Essay)
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

                // 🔒 Kiểm tra quyền Teacher nếu có
                if (teacherId.HasValue)
                {
                    if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId.Value, existingEssay.AssessmentId))
                    {
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Teacher không có quyền xóa Essay này";
                        return response;
                    }
                }

                // Lưu keys trước khi xóa
                string? audioKey = existingEssay.AudioKey;
                string? imageKey = existingEssay.ImageKey;

                // Xóa Essay trong database
                await _essayRepository.DeleteEssayAsync(essayId);

                // Xóa files trong MinIO sau khi xóa database thành công
                if (!string.IsNullOrWhiteSpace(audioKey))
                {
                    await _minioFileStorage.DeleteFileAsync(audioKey, EssayAudioBucket);
                    _logger.LogInformation("Deleted audio file {AudioKey} for Essay {EssayId}", audioKey, essayId);
                }

                if (!string.IsNullOrWhiteSpace(imageKey))
                {
                    await _minioFileStorage.DeleteFileAsync(imageKey, EssayImageBucket);
                    _logger.LogInformation("Deleted image file {ImageKey} for Essay {EssayId}", imageKey, essayId);
                }

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