using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Essay;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.EssayService
{
    
    public class AdminEssayService : IAdminEssayService
    {
        private readonly IEssayRepository _essayRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminEssayService> _logger;
        private readonly IEssayMediaService _essayMediaService;

        public AdminEssayService(
            IEssayRepository essayRepository,
            IMapper mapper,
            ILogger<AdminEssayService> logger,
            IEssayMediaService essayMediaService)
        {
            _essayRepository = essayRepository;
            _mapper = mapper;
            _logger = logger;
            _essayMediaService = essayMediaService;
        }

        public async Task<ServiceResponse<EssayDto>> AdminCreateEssay(CreateEssayDto dto)
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
                        try
                        {
                            committedAudioKey = await _essayMediaService.CommitAudioAsync(dto.AudioTempKey);
                            essay.AudioKey = committedAudioKey;
                            essay.AudioType = dto.AudioType;
                        }
                        catch (Exception audioEx)
                        {
                            _logger.LogError(audioEx, "Failed to commit essay audio");
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file audio. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Commit Image file nếu có
                    if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                    {
                        try
                        {
                            committedImageKey = await _essayMediaService.CommitImageAsync(dto.ImageTempKey);
                            essay.ImageKey = committedImageKey;
                            essay.ImageType = dto.ImageType;
                        }
                        catch (Exception imageEx)
                        {
                            _logger.LogError(imageEx, "Failed to commit essay image");
                            
                            // Rollback audio nếu đã commit
                            if (committedAudioKey != null)
                            {
                                await _essayMediaService.DeleteAudioAsync(committedAudioKey);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file hình ảnh. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Tạo Essay trong database
                    var createdEssay = await _essayRepository.CreateEssayAsync(essay);

                    // Map Entity to DTO
                    var essayDto = _mapper.Map<EssayDto>(createdEssay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(createdEssay.AudioKey))
                    {
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(createdEssay.AudioKey);
                        essayDto.AudioType = createdEssay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(createdEssay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(createdEssay.ImageKey);
                        essayDto.ImageType = createdEssay.ImageType;
                    }

                    response.Success = true;
                    response.StatusCode = 201;
                    response.Message = "Tạo Essay thành công";
                    response.Data = essayDto;

                    _logger.LogInformation("Admin created Essay {EssayId} with audio: {HasAudio}, image: {HasImage}",
                        createdEssay.EssayId,
                        !string.IsNullOrWhiteSpace(committedAudioKey),
                        !string.IsNullOrWhiteSpace(committedImageKey));

                    return response;
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating Essay");

                    // Rollback files if DB fails
                    if (committedAudioKey != null)
                    {
                        await _essayMediaService.DeleteAudioAsync(committedAudioKey);
                    }
                    if (committedImageKey != null)
                    {
                        await _essayMediaService.DeleteImageAsync(committedImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo Essay";
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Admin tạo Essay");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi tạo Essay";
                return response;
            }
        }

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
                    essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(essay.AudioKey);
                    essayDto.AudioType = essay.AudioType;
                }

                if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                {
                    essayDto.ImageUrl = _essayMediaService.BuildImageUrl(essay.ImageKey);
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
                _logger.LogError(ex, "Lỗi khi Admin lấy thông tin Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy thông tin Essay";
                return response;
            }
        }

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
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(essay.AudioKey);
                        essayDto.AudioType = essay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(essay.ImageKey);
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
                _logger.LogError(ex, "Lỗi khi Admin lấy danh sách Essay theo Assessment ID {AssessmentId}", assessmentId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách Essay";
                return response;
            }
        }

        public async Task<ServiceResponse<EssayDto>> UpdateEssay(int essayId, UpdateEssayDto dto)
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
                        try
                        {
                            newAudioKey = await _essayMediaService.CommitAudioAsync(dto.AudioTempKey);
                            existingEssay.AudioKey = newAudioKey;
                            existingEssay.AudioType = dto.AudioType;
                        }
                        catch (Exception audioEx)
                        {
                            _logger.LogError(audioEx, "Failed to commit new essay audio");
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file audio mới. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Xử lý Image file mới
                    if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                    {
                        try
                        {
                            newImageKey = await _essayMediaService.CommitImageAsync(dto.ImageTempKey);
                            existingEssay.ImageKey = newImageKey;
                            existingEssay.ImageType = dto.ImageType;
                        }
                        catch (Exception imageEx)
                        {
                            _logger.LogError(imageEx, "Failed to commit new essay image");
                            
                            // Rollback audio nếu đã commit
                            if (newAudioKey != null && newAudioKey != oldAudioKey)
                            {
                                await _essayMediaService.DeleteAudioAsync(newAudioKey);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file hình ảnh mới. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Update database
                    var updatedEssay = await _essayRepository.UpdateEssayAsync(existingEssay);

                    // Xóa file cũ sau khi update thành công
                    if (newAudioKey != null && oldAudioKey != null && newAudioKey != oldAudioKey)
                    {
                        await _essayMediaService.DeleteAudioAsync(oldAudioKey);
                    }

                    if (newImageKey != null && oldImageKey != null && newImageKey != oldImageKey)
                    {
                        await _essayMediaService.DeleteImageAsync(oldImageKey);
                    }

                    // Map to DTO và generate URLs
                    var essayDto = _mapper.Map<EssayDto>(updatedEssay);

                    if (!string.IsNullOrWhiteSpace(updatedEssay.AudioKey))
                    {
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(updatedEssay.AudioKey);
                        essayDto.AudioType = updatedEssay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(updatedEssay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(updatedEssay.ImageKey);
                        essayDto.ImageType = updatedEssay.ImageType;
                    }

                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Cập nhật Essay thành công";
                    response.Data = essayDto;

                    _logger.LogInformation("Admin updated Essay {EssayId}", essayId);

                    return response;
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating Essay");

                    // Rollback new files if DB fails
                    if (newAudioKey != null && newAudioKey != oldAudioKey)
                    {
                        await _essayMediaService.DeleteAudioAsync(newAudioKey);
                    }
                    if (newImageKey != null && newImageKey != oldImageKey)
                    {
                        await _essayMediaService.DeleteImageAsync(newImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật Essay";
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Admin cập nhật Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi cập nhật Essay";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteEssay(int essayId)
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

                // Lưu keys trước khi xóa
                string? audioKey = existingEssay.AudioKey;
                string? imageKey = existingEssay.ImageKey;

                // Xóa files trong MinIO trước khi xóa database
                if (!string.IsNullOrWhiteSpace(audioKey))
                {
                    await _essayMediaService.DeleteAudioAsync(audioKey);
                }

                if (!string.IsNullOrWhiteSpace(imageKey))
                {
                    await _essayMediaService.DeleteImageAsync(imageKey);
                }

                // Xóa Essay trong database
                await _essayRepository.DeleteEssayAsync(essayId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Essay thành công";
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Admin xóa Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi xóa Essay";
                return response;
            }
        }
    }
}
