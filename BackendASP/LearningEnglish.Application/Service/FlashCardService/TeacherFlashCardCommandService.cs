using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.FlashCardService
{
    public class TeacherFlashCardCommandService : ITeacherFlashCardCommandService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherFlashCardCommandService> _logger;
        private readonly IMinioFileStorage? _minioFileStorage;

        // MinIO bucket constants
        private const string AUDIO_BUCKET_NAME = "flashcard-audio";
        private const string IMAGE_BUCKET_NAME = "flashcards";
        private const string FlashCardFolder = "real";

        public TeacherFlashCardCommandService(
            IFlashCardRepository flashCardRepository,
            IModuleRepository moduleRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherFlashCardCommandService> logger,
            IMinioFileStorage? minioFileStorage = null)
        {
            _flashCardRepository = flashCardRepository;
            _moduleRepository = moduleRepository;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
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
                    var imageResult = await _minioFileStorage!.CommitFileAsync(
                        createFlashCardDto.ImageTempKey,
                        IMAGE_BUCKET_NAME,
                        FlashCardFolder
                    );

                    if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                    {
                        _logger.LogError("Failed to commit image for flashcard. Error: {Error}", imageResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu hình ảnh: {imageResult.Message}";
                        return response;
                    }

                    flashCard.ImageKey = imageResult.Data;
                }

                // Handle audio upload
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.AudioTempKey))
                {
                    var audioResult = await _minioFileStorage!.CommitFileAsync(
                        createFlashCardDto.AudioTempKey,
                        AUDIO_BUCKET_NAME,
                        FlashCardFolder
                    );

                    if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                    {
                        _logger.LogError("Failed to commit audio for flashcard. Error: {Error}", audioResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu audio: {audioResult.Message}";
                        return response;
                    }

                    flashCard.AudioKey = audioResult.Data;
                }

                var createdFlashCard = await _flashCardRepository.CreateAsync(flashCard);

                var flashCardDto = _mapper.Map<FlashCardDto>(createdFlashCard);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCardDto.AudioUrl);
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
                        var imageResult = await _minioFileStorage!.CommitFileAsync(
                            flashCardDto.ImageTempKey,
                            IMAGE_BUCKET_NAME,
                            FlashCardFolder
                        );

                        if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                        {
                            _logger.LogError("Failed to commit image for flashcard {Word}. Error: {Error}", flashCardDto.Word, imageResult.Message);

                            // Rollback committed images
                            foreach (var key in committedImageKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, IMAGE_BUCKET_NAME);
                            }
                            foreach (var key in committedAudioKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, AUDIO_BUCKET_NAME);
                            }

                            response.Success = false;
                            response.Message = $"Không thể lưu hình ảnh cho '{flashCardDto.Word}': {imageResult.Message}";
                            return response;
                        }

                        flashCard.ImageKey = imageResult.Data;
                        committedImageKeys.Add(imageResult.Data);
                    }

                    // Handle audio upload
                    if (!string.IsNullOrWhiteSpace(flashCardDto.AudioTempKey))
                    {
                        var audioResult = await _minioFileStorage!.CommitFileAsync(
                            flashCardDto.AudioTempKey,
                            AUDIO_BUCKET_NAME,
                            FlashCardFolder
                        );

                        if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                        {
                            _logger.LogError("Failed to commit audio for flashcard {Word}. Error: {Error}", flashCardDto.Word, audioResult.Message);

                            // Rollback committed files
                            foreach (var key in committedImageKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, IMAGE_BUCKET_NAME);
                            }
                            foreach (var key in committedAudioKeys)
                            {
                                await _minioFileStorage.DeleteFileAsync(key, AUDIO_BUCKET_NAME);
                            }

                            response.Success = false;
                            response.Message = $"Không thể lưu audio cho '{flashCardDto.Word}': {audioResult.Message}";
                            return response;
                        }

                        flashCard.AudioKey = audioResult.Data;
                        committedAudioKeys.Add(audioResult.Data);
                    }

                    flashCards.Add(flashCard);
                }

                var createdFlashCards = await _flashCardRepository.CreateBulkAsync(flashCards);

                var flashCardDtos = createdFlashCards.Select(fc =>
                {
                    var dto = _mapper.Map<FlashCardDto>(fc);
                    if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, dto.ImageUrl);
                    }
                    if (!string.IsNullOrWhiteSpace(dto.AudioUrl))
                    {
                        dto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, dto.AudioUrl);
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
                    var imageResult = await _minioFileStorage!.CommitFileAsync(
                        updateFlashCardDto.ImageTempKey,
                        IMAGE_BUCKET_NAME,
                        FlashCardFolder
                    );

                    if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                    {
                        _logger.LogError("Failed to commit image for flashcard update. Error: {Error}", imageResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu hình ảnh: {imageResult.Message}";
                        return response;
                    }

                    committedImageKey = imageResult.Data;
                }

                // Handle audio update
                string? committedAudioKey = flashCardToUpdate.AudioKey;
                string? oldAudioKey = flashCardToUpdate.AudioKey;
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.AudioTempKey))
                {
                    var audioResult = await _minioFileStorage!.CommitFileAsync(
                        updateFlashCardDto.AudioTempKey,
                        AUDIO_BUCKET_NAME,
                        FlashCardFolder
                    );

                    if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                    {
                        _logger.LogError("Failed to commit audio for flashcard update. Error: {Error}", audioResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu audio: {audioResult.Message}";
                        return response;
                    }

                    committedAudioKey = audioResult.Data;
                }

                // Update flashcard properties
                _mapper.Map(updateFlashCardDto, flashCardToUpdate);
                flashCardToUpdate.ImageKey = committedImageKey;
                flashCardToUpdate.AudioKey = committedAudioKey;

                var updatedFlashCard = await _flashCardRepository.UpdateAsync(flashCardToUpdate);

                // Xóa file cũ chỉ sau khi update thành công
                if (!string.IsNullOrWhiteSpace(oldImageKey) && committedImageKey != null && oldImageKey != committedImageKey)
                {
                    try
                    {
                        await _minioFileStorage!.DeleteFileAsync(oldImageKey, IMAGE_BUCKET_NAME);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete old flashcard image: {ImageKey}", oldImageKey);
                    }
                }

                if (!string.IsNullOrWhiteSpace(oldAudioKey) && committedAudioKey != null && oldAudioKey != committedAudioKey)
                {
                    try
                    {
                        await _minioFileStorage!.DeleteFileAsync(oldAudioKey, AUDIO_BUCKET_NAME);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete old flashcard audio: {AudioKey}", oldAudioKey);
                    }
                }

                var flashCardDto = _mapper.Map<FlashCardDto>(updatedFlashCard);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCardDto.AudioUrl);
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
                    var deleteImageResult = await _minioFileStorage!.DeleteFileAsync(flashCard.ImageKey, IMAGE_BUCKET_NAME);
                    if (!deleteImageResult.Success)
                    {
                        _logger.LogWarning("Failed to delete image file {ImageKey} for flashcard {FlashCardId}", flashCard.ImageKey, flashCardId);
                    }
                }

                // Xóa audio từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(flashCard.AudioKey))
                {
                    var deleteAudioResult = await _minioFileStorage!.DeleteFileAsync(flashCard.AudioKey, AUDIO_BUCKET_NAME);
                    if (!deleteAudioResult.Success)
                    {
                        _logger.LogWarning("Failed to delete audio file {AudioKey} for flashcard {FlashCardId}", flashCard.AudioKey, flashCardId);
                    }
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