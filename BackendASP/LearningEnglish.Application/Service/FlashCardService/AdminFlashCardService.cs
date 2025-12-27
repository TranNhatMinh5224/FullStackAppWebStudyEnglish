using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminFlashCardService : IAdminFlashCardService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminFlashCardService> _logger;
        private readonly IMinioFileStorage? _minioFileStorage;

        private const string AUDIO_BUCKET = "flashcard-audio";
        private const string IMAGE_BUCKET = "flashcards";
        private const string FLASHCARD_FOLDER = "real";

        public AdminFlashCardService(
            IFlashCardRepository flashCardRepository,
            IMapper mapper,
            ILogger<AdminFlashCardService> logger,
            IMinioFileStorage? minioFileStorage = null)
        {
            _flashCardRepository = flashCardRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
        }

        // Admin tạo flashcard
        public async Task<ServiceResponse<FlashCardDto>> AdminCreateFlashCard(CreateFlashCardDto dto)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                var flashCard = _mapper.Map<FlashCard>(dto);

                string? imageKey = null;
                string? audioKey = null;

                // Commit image nếu có
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey) && _minioFileStorage != null)
                {
                    var imageResult = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey, IMAGE_BUCKET, FLASHCARD_FOLDER);
                    
                    if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                        return response.Fail(400, "Không thể lưu image");
                    
                    imageKey = imageResult.Data;
                    flashCard.ImageKey = imageKey;
                }

                // Commit audio nếu có
                if (!string.IsNullOrWhiteSpace(dto.AudioTempKey) && _minioFileStorage != null)
                {
                    var audioResult = await _minioFileStorage.CommitFileAsync(
                        dto.AudioTempKey, AUDIO_BUCKET, FLASHCARD_FOLDER);
                    
                    if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                    {
                        // Rollback image if audio fails
                        if (imageKey != null)
                        {
                            try
                            {
                                await _minioFileStorage.DeleteFileAsync(imageKey, IMAGE_BUCKET);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to rollback image {Key}", imageKey);
                            }
                        }
                        return response.Fail(400, "Không thể lưu audio");
                    }
                    
                    audioKey = audioResult.Data;
                    flashCard.AudioKey = audioKey;
                }

                var created = await _flashCardRepository.CreateAsync(flashCard);

                // Map DTO inline
                var result = _mapper.Map<FlashCardDto>(created);
                if (!string.IsNullOrWhiteSpace(created.ImageKey))
                    result.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET, created.ImageKey);
                if (!string.IsNullOrWhiteSpace(created.AudioKey))
                    result.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET, created.AudioKey);

                return response.SuccessResult(201, "Tạo FlashCard thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create FlashCard failed for ModuleId: {ModuleId}. Error: {Error}", 
                    dto.ModuleId, ex.ToString());
                return response.Fail(500, "Có lỗi xảy ra khi tạo FlashCard");
            }
        }

        // Admin import flashcards hàng loạt
        public async Task<ServiceResponse<List<FlashCardDto>>> AdminBulkCreateFlashCards(BulkImportFlashCardDto dto)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();

            try
            {
                var entities = dto.FlashCards.Select(x =>
                {
                    var fc = _mapper.Map<FlashCard>(x);
                    fc.ModuleId = dto.ModuleId;
                    return fc;
                }).ToList();

                var created = await _flashCardRepository.CreateBulkAsync(entities);
                
                // Map DTO inline
                var result = created.Select(fc =>
                {
                    var d = _mapper.Map<FlashCardDto>(fc);
                    if (!string.IsNullOrWhiteSpace(fc.ImageKey))
                        d.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET, fc.ImageKey);
                    if (!string.IsNullOrWhiteSpace(fc.AudioKey))
                        d.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET, fc.AudioKey);
                    return d;
                }).ToList();

                return response.SuccessResult(201, "Import flashcards thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk import flashcards failed for ModuleId: {ModuleId}. Error: {Error}", 
                    dto.ModuleId, ex.ToString());
                return response.Fail(500, "Có lỗi khi import flashcards");
            }
        }

        // Admin cập nhật flashcard
        public async Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto dto)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                var flashCard = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (flashCard == null)
                    return response.Fail(404, "Không tìm thấy FlashCard");

                var oldImageKey = flashCard.ImageKey;
                var oldAudioKey = flashCard.AudioKey;

                _mapper.Map(dto, flashCard);

                string? newImageKey = null;
                string? newAudioKey = null;

                // Commit image mới
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey) && _minioFileStorage != null)
                {
                    var imageResult = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey, IMAGE_BUCKET, FLASHCARD_FOLDER);
                    
                    if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                        return response.Fail(400, "Không thể lưu image mới");
                    
                    newImageKey = imageResult.Data;
                    flashCard.ImageKey = newImageKey;
                }

                // Commit audio mới
                if (!string.IsNullOrWhiteSpace(dto.AudioTempKey) && _minioFileStorage != null)
                {
                    var audioResult = await _minioFileStorage.CommitFileAsync(
                        dto.AudioTempKey, AUDIO_BUCKET, FLASHCARD_FOLDER);
                    
                    if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                    {
                        // Rollback new image
                        if (newImageKey != null)
                        {
                            try
                            {
                                await _minioFileStorage.DeleteFileAsync(newImageKey, IMAGE_BUCKET);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to rollback image {Key}", newImageKey);
                            }
                        }
                        return response.Fail(400, "Không thể lưu audio mới");
                    }
                    
                    newAudioKey = audioResult.Data;
                    flashCard.AudioKey = newAudioKey;
                }

                var updated = await _flashCardRepository.UpdateAsync(flashCard);

                // Xóa file cũ sau khi update DB thành công
                if (oldImageKey != null && newImageKey != null && _minioFileStorage != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, IMAGE_BUCKET);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old image {Key}", oldImageKey);
                    }
                }

                if (oldAudioKey != null && newAudioKey != null && _minioFileStorage != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(oldAudioKey, AUDIO_BUCKET);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete old audio {Key}", oldAudioKey);
                    }
                }

                // Map DTO inline
                var result = _mapper.Map<FlashCardDto>(updated);
                if (!string.IsNullOrWhiteSpace(updated.ImageKey))
                    result.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET, updated.ImageKey);
                if (!string.IsNullOrWhiteSpace(updated.AudioKey))
                    result.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET, updated.AudioKey);

                return response.SuccessResult(200, "Cập nhật FlashCard thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update FlashCard failed for FlashCardId: {FlashCardId}. Error: {Error}", 
                    flashCardId, ex.ToString());
                return response.Fail(500, "Có lỗi khi cập nhật FlashCard");
            }
        }

        // Admin xóa flashcard
        public async Task<ServiceResponse<bool>> DeleteFlashCard(int flashCardId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var flashCard = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (flashCard == null)
                    return response.Fail(404, "Không tìm thấy FlashCard");

                await _flashCardRepository.DeleteAsync(flashCardId);

                // Best-effort cleanup của files
                if (!string.IsNullOrWhiteSpace(flashCard.ImageKey) && _minioFileStorage != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(flashCard.ImageKey, IMAGE_BUCKET);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image {Key}", flashCard.ImageKey);
                    }
                }

                if (!string.IsNullOrWhiteSpace(flashCard.AudioKey) && _minioFileStorage != null)
                {
                    try
                    {
                        await _minioFileStorage.DeleteFileAsync(flashCard.AudioKey, AUDIO_BUCKET);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete audio {Key}", flashCard.AudioKey);
                    }
                }

                return response.SuccessResult(200, "Xóa FlashCard thành công", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete FlashCard failed for FlashCardId: {FlashCardId}. Error: {Error}", 
                    flashCardId, ex.ToString());
                return response.Fail(500, "Có lỗi khi xóa FlashCard");
            }
        }

        // Admin lấy flashcard theo ID
        public async Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                var flashCard = await _flashCardRepository.GetByIdWithDetailsAsync(flashCardId);
                if (flashCard == null)
                    return response.Fail(404, "Không tìm thấy FlashCard");

                // Map DTO inline
                var result = _mapper.Map<FlashCardDto>(flashCard);
                if (!string.IsNullOrWhiteSpace(flashCard.ImageKey))
                    result.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET, flashCard.ImageKey);
                if (!string.IsNullOrWhiteSpace(flashCard.AudioKey))
                    result.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET, flashCard.AudioKey);

                return response.SuccessResult(200, "Lấy FlashCard thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get FlashCard by ID failed for FlashCardId: {FlashCardId}. Error: {Error}", 
                    flashCardId, ex.ToString());
                return response.Fail(500, "Có lỗi khi lấy FlashCard");
            }
        }

        // Admin lấy danh sách flashcard theo module
        public async Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId)
        {
            var response = new ServiceResponse<List<ListFlashCardDto>>();

            try
            {
                var flashCards = await _flashCardRepository.GetByModuleIdWithDetailsAsync(moduleId);
                var result = flashCards.Select(fc =>
                {
                    var dto = _mapper.Map<ListFlashCardDto>(fc);
                    if (!string.IsNullOrWhiteSpace(fc.ImageKey))
                        dto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET, fc.ImageKey);
                    if (!string.IsNullOrWhiteSpace(fc.AudioKey))
                        dto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET, fc.AudioKey);
                    return dto;
                }).ToList();

                return response.SuccessResult(200, "Lấy danh sách FlashCard thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get FlashCards by ModuleId failed for ModuleId: {ModuleId}. Error: {Error}", 
                    moduleId, ex.ToString());
                return response.Fail(500, "Có lỗi khi lấy danh sách FlashCard");
            }
        }
    }
}
