using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    /// <summary>
    /// Admin flashcard service following SOLID principles
    /// Uses shared media service to reduce code duplication (DRY)
    /// </summary>
    public class AdminFlashCardService : IAdminFlashCardService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminFlashCardService> _logger;
        private readonly IFlashCardMediaService _flashCardMediaService;

        public AdminFlashCardService(
            IFlashCardRepository flashCardRepository,
            IMapper mapper,
            ILogger<AdminFlashCardService> logger,
            IFlashCardMediaService flashCardMediaService)
        {
            _flashCardRepository = flashCardRepository;
            _mapper = mapper;
            _logger = logger;
            _flashCardMediaService = flashCardMediaService;
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
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    try
                    {
                        imageKey = await _flashCardMediaService.CommitImageAsync(dto.ImageTempKey);
                        flashCard.ImageKey = imageKey;
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit flashcard image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu image. Vui lòng thử lại.";
                        return response;
                    }
                }

                // Commit audio nếu có
                if (!string.IsNullOrWhiteSpace(dto.AudioTempKey))
                {
                    try
                    {
                        audioKey = await _flashCardMediaService.CommitAudioAsync(dto.AudioTempKey);
                        flashCard.AudioKey = audioKey;
                    }
                    catch (Exception audioEx)
                    {
                        _logger.LogError(audioEx, "Failed to commit flashcard audio");
                        
                        // Rollback image if audio fails
                        if (imageKey != null)
                        {
                            await _flashCardMediaService.DeleteImageAsync(imageKey);
                        }
                        
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu audio. Vui lòng thử lại.";
                        return response;
                    }
                }

                var created = await _flashCardRepository.CreateAsync(flashCard);

                // Map DTO inline
                var result = _mapper.Map<FlashCardDto>(created);
                if (!string.IsNullOrWhiteSpace(created.ImageKey))
                    result.ImageUrl = _flashCardMediaService.BuildImageUrl(created.ImageKey);
                if (!string.IsNullOrWhiteSpace(created.AudioKey))
                    result.AudioUrl = _flashCardMediaService.BuildAudioUrl(created.AudioKey);

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Tạo FlashCard thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create FlashCard failed for ModuleId: {ModuleId}. Error: {Error}", 
                    dto.ModuleId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi tạo FlashCard";
                return response;
            }
        }

        // Admin import flashcards hàng loạt
        public async Task<ServiceResponse<List<FlashCardDto>>> AdminBulkCreateFlashCards(BulkImportFlashCardDto dto)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();

            try
            {
                var flashCards = new List<FlashCard>();
                var committedImageKeys = new List<string>();
                var committedAudioKeys = new List<string>();

                foreach (var flashCardDto in dto.FlashCards)
                {
                    var flashCard = _mapper.Map<FlashCard>(flashCardDto);
                    flashCard.ModuleId = dto.ModuleId;

                    // Handle image upload
                    if (!string.IsNullOrWhiteSpace(flashCardDto.ImageTempKey))
                    {
                        try
                        {
                            flashCard.ImageKey = await _flashCardMediaService.CommitImageAsync(flashCardDto.ImageTempKey);
                            committedImageKeys.Add(flashCard.ImageKey);
                        }
                        catch (Exception imageEx)
                        {
                            _logger.LogError(imageEx, "Failed to commit image for flashcard {Word}", flashCardDto.Word);

                            // Rollback committed images
                            foreach (var key in committedImageKeys)
                            {
                                await _flashCardMediaService.DeleteImageAsync(key);
                            }
                            foreach (var key in committedAudioKeys)
                            {
                                await _flashCardMediaService.DeleteAudioAsync(key);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = $"Không thể lưu hình ảnh cho '{flashCardDto.Word}'. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Handle audio upload
                    if (!string.IsNullOrWhiteSpace(flashCardDto.AudioTempKey))
                    {
                        try
                        {
                            flashCard.AudioKey = await _flashCardMediaService.CommitAudioAsync(flashCardDto.AudioTempKey);
                            committedAudioKeys.Add(flashCard.AudioKey);
                        }
                        catch (Exception audioEx)
                        {
                            _logger.LogError(audioEx, "Failed to commit audio for flashcard {Word}", flashCardDto.Word);

                            // Rollback committed files
                            foreach (var key in committedImageKeys)
                            {
                                await _flashCardMediaService.DeleteImageAsync(key);
                            }
                            foreach (var key in committedAudioKeys)
                            {
                                await _flashCardMediaService.DeleteAudioAsync(key);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = $"Không thể lưu audio cho '{flashCardDto.Word}'. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    flashCards.Add(flashCard);
                }

                var created = await _flashCardRepository.CreateBulkAsync(flashCards);
                
                // Map DTO inline
                var result = created.Select(fc =>
                {
                    var d = _mapper.Map<FlashCardDto>(fc);
                    if (!string.IsNullOrWhiteSpace(fc.ImageKey))
                        d.ImageUrl = _flashCardMediaService.BuildImageUrl(fc.ImageKey);
                    if (!string.IsNullOrWhiteSpace(fc.AudioKey))
                        d.AudioUrl = _flashCardMediaService.BuildAudioUrl(fc.AudioKey);
                    return d;
                }).ToList();

                response.Success = true;
                response.StatusCode = 201;
                response.Message = "Import flashcards thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk import flashcards failed for ModuleId: {ModuleId}. Error: {Error}", 
                    dto.ModuleId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi khi import flashcards";
                return response;
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
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
                }

                var oldImageKey = flashCard.ImageKey;
                var oldAudioKey = flashCard.AudioKey;

                _mapper.Map(dto, flashCard);

                string? newImageKey = null;
                string? newAudioKey = null;

                // Commit image mới
                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    try
                    {
                        newImageKey = await _flashCardMediaService.CommitImageAsync(dto.ImageTempKey);
                        flashCard.ImageKey = newImageKey;
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit new flashcard image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu image mới. Vui lòng thử lại.";
                        return response;
                    }
                }

                // Commit audio mới
                if (!string.IsNullOrWhiteSpace(dto.AudioTempKey))
                {
                    try
                    {
                        newAudioKey = await _flashCardMediaService.CommitAudioAsync(dto.AudioTempKey);
                        flashCard.AudioKey = newAudioKey;
                    }
                    catch (Exception audioEx)
                    {
                        _logger.LogError(audioEx, "Failed to commit new flashcard audio");
                        
                        // Rollback new image if audio fails
                        if (newImageKey != null)
                        {
                            await _flashCardMediaService.DeleteImageAsync(newImageKey);
                        }
                        
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu audio mới. Vui lòng thử lại.";
                        return response;
                    }
                }

                var updated = await _flashCardRepository.UpdateAsync(flashCard);

                // Xóa file cũ sau khi update DB thành công
                if (oldImageKey != null && newImageKey != null)
                {
                    await _flashCardMediaService.DeleteImageAsync(oldImageKey);
                }

                if (oldAudioKey != null && newAudioKey != null)
                {
                    await _flashCardMediaService.DeleteAudioAsync(oldAudioKey);
                }

                // Map DTO inline
                var result = _mapper.Map<FlashCardDto>(updated);
                if (!string.IsNullOrWhiteSpace(updated.ImageKey))
                    result.ImageUrl = _flashCardMediaService.BuildImageUrl(updated.ImageKey);
                if (!string.IsNullOrWhiteSpace(updated.AudioKey))
                    result.AudioUrl = _flashCardMediaService.BuildAudioUrl(updated.AudioKey);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Cập nhật FlashCard thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update FlashCard failed for FlashCardId: {FlashCardId}. Error: {Error}", 
                    flashCardId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi khi cập nhật FlashCard";
                return response;
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
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
                }

                await _flashCardRepository.DeleteAsync(flashCardId);

                // Best-effort cleanup của files
                if (!string.IsNullOrWhiteSpace(flashCard.ImageKey))
                {
                    await _flashCardMediaService.DeleteImageAsync(flashCard.ImageKey);
                }

                if (!string.IsNullOrWhiteSpace(flashCard.AudioKey))
                {
                    await _flashCardMediaService.DeleteAudioAsync(flashCard.AudioKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa FlashCard thành công";
                response.Data = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete FlashCard failed for FlashCardId: {FlashCardId}. Error: {Error}", 
                    flashCardId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi khi xóa FlashCard";
                return response;
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
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
                }

                // Map DTO inline
                var result = _mapper.Map<FlashCardDto>(flashCard);
                if (!string.IsNullOrWhiteSpace(flashCard.ImageKey))
                    result.ImageUrl = _flashCardMediaService.BuildImageUrl(flashCard.ImageKey);
                if (!string.IsNullOrWhiteSpace(flashCard.AudioKey))
                    result.AudioUrl = _flashCardMediaService.BuildAudioUrl(flashCard.AudioKey);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy FlashCard thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get FlashCard by ID failed for FlashCardId: {FlashCardId}. Error: {Error}", 
                    flashCardId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi khi lấy FlashCard";
                return response;
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
                        dto.ImageUrl = _flashCardMediaService.BuildImageUrl(fc.ImageKey);
                    if (!string.IsNullOrWhiteSpace(fc.AudioKey))
                        dto.AudioUrl = _flashCardMediaService.BuildAudioUrl(fc.AudioKey);
                    return dto;
                }).ToList();

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách FlashCard thành công";
                response.Data = result;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get FlashCards by ModuleId failed for ModuleId: {ModuleId}. Error: {Error}", 
                    moduleId, ex.ToString());
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi khi lấy danh sách FlashCard";
                return response;
            }
        }
    }
}
