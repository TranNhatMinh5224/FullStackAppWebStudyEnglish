using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.FlashCardService
{
    /// <summary>
    /// Teacher flashcard command service following SOLID principles
    /// Uses shared media service to reduce code duplication (DRY)
    /// </summary>
    public class TeacherFlashCardCommandService : ITeacherFlashCardCommandService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherFlashCardCommandService> _logger;
        private readonly IFlashCardMediaService _flashCardMediaService;

        public TeacherFlashCardCommandService(
            IFlashCardRepository flashCardRepository,
            IModuleRepository moduleRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherFlashCardCommandService> logger,
            IFlashCardMediaService flashCardMediaService)
        {
            _flashCardRepository = flashCardRepository;
            _moduleRepository = moduleRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _flashCardMediaService = flashCardMediaService;
        }

        // Teacher tạo flash card
        public async Task<ServiceResponse<FlashCardDto>> TeacherCreateFlashCard(CreateFlashCardDto createFlashCardDto, int teacherId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                if (!createFlashCardDto.ModuleId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "ModuleId là bắt buộc";
                    return response;
                }

                // Kiểm tra module ownership
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(createFlashCardDto.ModuleId.Value, teacherId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create flashcard for module {ModuleId} without ownership", 
                        teacherId, createFlashCardDto.ModuleId.Value);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được tạo flashcard
                if (module.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể tạo flashcard cho khóa học của giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create flashcard for System course module {ModuleId}", 
                        teacherId, createFlashCardDto.ModuleId.Value);
                    return response;
                }

                var flashCard = _mapper.Map<FlashCard>(createFlashCardDto);

                // Handle image upload
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.ImageTempKey))
                {
                    try
                    {
                        flashCard.ImageKey = await _flashCardMediaService.CommitImageAsync(createFlashCardDto.ImageTempKey);
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit image for flashcard");
                        response.Success = false;
                        response.Message = "Không thể lưu hình ảnh. Vui lòng thử lại.";
                        return response;
                    }
                }

                // Handle audio upload
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.AudioTempKey))
                {
                    try
                    {
                        flashCard.AudioKey = await _flashCardMediaService.CommitAudioAsync(createFlashCardDto.AudioTempKey);
                    }
                    catch (Exception audioEx)
                    {
                        _logger.LogError(audioEx, "Failed to commit audio for flashcard");
                        
                        // Rollback image if audio fails
                        if (!string.IsNullOrWhiteSpace(flashCard.ImageKey))
                        {
                            await _flashCardMediaService.DeleteImageAsync(flashCard.ImageKey);
                        }
                        
                        response.Success = false;
                        response.Message = "Không thể lưu audio. Vui lòng thử lại.";
                        return response;
                    }
                }

                var createdFlashCard = await _flashCardRepository.CreateAsync(flashCard);

                var flashCardDto = _mapper.Map<FlashCardDto>(createdFlashCard);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(createdFlashCard.ImageKey))
                {
                    flashCardDto.ImageUrl = _flashCardMediaService.BuildImageUrl(createdFlashCard.ImageKey);
                }
                if (!string.IsNullOrWhiteSpace(createdFlashCard.AudioKey))
                {
                    flashCardDto.AudioUrl = _flashCardMediaService.BuildAudioUrl(createdFlashCard.AudioKey);
                }

                response.Success = true;
                response.StatusCode = 201;
                response.Data = flashCardDto;
                response.Message = "Tạo flash card thành công";
                _logger.LogInformation("Teacher {TeacherId} đã tạo flashcard {FlashCardId} thành công", teacherId, createdFlashCard.FlashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo flashcard cho teacher {TeacherId}", teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo flashcard";
            }

            return response;
        }

        // Teacher tạo nhiều flashcard
        public async Task<ServiceResponse<List<FlashCardDto>>> TeacherBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto, int teacherId)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();

            try
            {
                // Kiểm tra module ownership
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(bulkImportDto.ModuleId, teacherId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to bulk create flashcards for module {ModuleId} without ownership", 
                        teacherId, bulkImportDto.ModuleId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được tạo flashcard
                if (module.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể tạo flashcard cho khóa học của giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to bulk create flashcards for System course module {ModuleId}", 
                        teacherId, bulkImportDto.ModuleId);
                    return response;
                }

                var flashCards = new List<FlashCard>();
                var committedImageKeys = new List<string>();
                var committedAudioKeys = new List<string>();

                foreach (var flashCardDto in bulkImportDto.FlashCards)
                {
                    var flashCard = _mapper.Map<FlashCard>(flashCardDto);
                    flashCard.ModuleId = bulkImportDto.ModuleId;

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
                            response.Message = $"Không thể lưu audio cho '{flashCardDto.Word}'. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    flashCards.Add(flashCard);
                }

                var createdFlashCards = await _flashCardRepository.CreateBulkAsync(flashCards);

                var flashCardDtos = createdFlashCards.Select(fc =>
                {
                    var dto = _mapper.Map<FlashCardDto>(fc);
                    if (!string.IsNullOrWhiteSpace(fc.ImageKey))
                    {
                        dto.ImageUrl = _flashCardMediaService.BuildImageUrl(fc.ImageKey);
                    }
                    if (!string.IsNullOrWhiteSpace(fc.AudioKey))
                    {
                        dto.AudioUrl = _flashCardMediaService.BuildAudioUrl(fc.AudioKey);
                    }
                    return dto;
                }).ToList();

                response.Success = true;
                response.StatusCode = 201;
                response.Data = flashCardDtos;
                response.Message = $"Tạo thành công {flashCardDtos.Count} flashcard(s)";
                _logger.LogInformation("Teacher {TeacherId} đã bulk create {Count} flashcards thành công", teacherId, flashCardDtos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi bulk create flashcards cho teacher {TeacherId}", teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo nhiều flashcards";
            }

            return response;
        }

        // Cập nhật flashcard
        public async Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int teacherId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                // Validate ownership và load với navigation properties để check CourseType
                var existingFlashCard = await _flashCardRepository.GetByIdWithDetailsForTeacherAsync(flashCardId, teacherId);
                if (existingFlashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update flashcard {FlashCardId} without ownership", 
                        teacherId, flashCardId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được cập nhật flashcard
                if (existingFlashCard.Module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể cập nhật flashcard của khóa học giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update flashcard {FlashCardId} of System course", 
                        teacherId, flashCardId);
                    return response;
                }

                // Load flashcard để update (không cần navigation properties nữa)
                var flashCardToUpdate = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (flashCardToUpdate == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard";
                    return response;
                }

                // Handle image update
                string? committedImageKey = flashCardToUpdate.ImageKey;
                string? oldImageKey = flashCardToUpdate.ImageKey;
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.ImageTempKey))
                {
                    try
                    {
                        committedImageKey = await _flashCardMediaService.CommitImageAsync(updateFlashCardDto.ImageTempKey);
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit image for flashcard update");
                        response.Success = false;
                        response.Message = "Không thể lưu hình ảnh. Vui lòng thử lại.";
                        return response;
                    }
                }

                // Handle audio update
                string? committedAudioKey = flashCardToUpdate.AudioKey;
                string? oldAudioKey = flashCardToUpdate.AudioKey;
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.AudioTempKey))
                {
                    try
                    {
                        committedAudioKey = await _flashCardMediaService.CommitAudioAsync(updateFlashCardDto.AudioTempKey);
                    }
                    catch (Exception audioEx)
                    {
                        _logger.LogError(audioEx, "Failed to commit audio for flashcard update");
                        
                        // Rollback new image if audio fails
                        if (committedImageKey != null && oldImageKey != committedImageKey)
                        {
                            await _flashCardMediaService.DeleteImageAsync(committedImageKey);
                        }
                        
                        response.Success = false;
                        response.Message = "Không thể lưu audio. Vui lòng thử lại.";
                        return response;
                    }
                }

                // Update flashcard properties
                _mapper.Map(updateFlashCardDto, flashCardToUpdate);
                flashCardToUpdate.ImageKey = committedImageKey;
                flashCardToUpdate.AudioKey = committedAudioKey;

                var updatedFlashCard = await _flashCardRepository.UpdateAsync(flashCardToUpdate);

                // Xóa file cũ chỉ sau khi update thành công
                if (!string.IsNullOrWhiteSpace(oldImageKey) && committedImageKey != null && oldImageKey != committedImageKey)
                {
                    await _flashCardMediaService.DeleteImageAsync(oldImageKey);
                }

                if (!string.IsNullOrWhiteSpace(oldAudioKey) && committedAudioKey != null && oldAudioKey != committedAudioKey)
                {
                    await _flashCardMediaService.DeleteAudioAsync(oldAudioKey);
                }

                var flashCardDto = _mapper.Map<FlashCardDto>(updatedFlashCard);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(updatedFlashCard.ImageKey))
                {
                    flashCardDto.ImageUrl = _flashCardMediaService.BuildImageUrl(updatedFlashCard.ImageKey);
                }
                if (!string.IsNullOrWhiteSpace(updatedFlashCard.AudioKey))
                {
                    flashCardDto.AudioUrl = _flashCardMediaService.BuildAudioUrl(updatedFlashCard.AudioKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = flashCardDto;
                response.Message = "Cập nhật flashcard thành công";
                _logger.LogInformation("Teacher {TeacherId} đã cập nhật flashcard {FlashCardId} thành công", teacherId, flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật flashcard {FlashCardId} cho teacher {TeacherId}", flashCardId, teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi cập nhật flashcard";
            }

            return response;
        }

        // Xóa flashcard
        public async Task<ServiceResponse<bool>> DeleteFlashCard(int flashCardId, int teacherId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Validate ownership và load với navigation properties để check CourseType
                var flashCard = await _flashCardRepository.GetByIdWithDetailsForTeacherAsync(flashCardId, teacherId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete flashcard {FlashCardId} without ownership", 
                        teacherId, flashCardId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được xóa flashcard
                if (flashCard.Module?.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể xóa flashcard của khóa học giáo viên";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete flashcard {FlashCardId} of System course", 
                        teacherId, flashCardId);
                    return response;
                }

                // Xóa image từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(flashCard.ImageKey))
                {
                    await _flashCardMediaService.DeleteImageAsync(flashCard.ImageKey);
                }

                // Xóa audio từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(flashCard.AudioKey))
                {
                    await _flashCardMediaService.DeleteAudioAsync(flashCard.AudioKey);
                }

                var deleteResult = await _flashCardRepository.DeleteAsync(flashCardId);
                if (!deleteResult)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa flashcard";
                    return response;
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = true;
                response.Message = "Xóa flashcard thành công";
                _logger.LogInformation("Teacher {TeacherId} đã xóa flashcard {FlashCardId} thành công", teacherId, flashCardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa flashcard {FlashCardId} cho teacher {TeacherId}", flashCardId, teacherId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa flashcard";
            }

            return response;
        }
    }
}