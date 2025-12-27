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
    public class TeacherFlashCardService : ITeacherFlashCardService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherFlashCardService> _logger;
        private readonly IMinioFileStorage? _minioFileStorage;

        // MinIO bucket constants
        private const string AUDIO_BUCKET_NAME = "flashcard-audio";
        private const string IMAGE_BUCKET_NAME = "flashcards";
        private const string FlashCardFolder = "real";

        public TeacherFlashCardService(
            IFlashCardRepository flashCardRepository,
            IModuleRepository moduleRepository,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherFlashCardService> logger,
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
                // RLS đã filter modules theo TeacherId (chỉ modules của teacher này)
                // Nếu module không tồn tại hoặc không thuộc về teacher → RLS sẽ filter → module == null
                if (!createFlashCardDto.ModuleId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "ModuleId là bắt buộc";
                    return response;
                }
                
                var module = await _moduleRepository.GetByIdAsync(createFlashCardDto.ModuleId.Value);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    return response;
                }

                // Defense in depth: Check ownership ở service layer (RLS cũng sẽ block nếu không đúng)
                var lesson = await _lessonRepository.GetLessonById(module.LessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }

                var course = await _courseRepository.GetCourseById(lesson.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                if (!course.TeacherId.HasValue || course.TeacherId.Value != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền tạo flash card trong module này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to create flashcard in module {ModuleId}, course {CourseId} owned by {OwnerId}",
                        teacherId, createFlashCardDto.ModuleId, lesson.CourseId, course.TeacherId);
                    return response;
                }

                var flashCard = _mapper.Map<FlashCard>(createFlashCardDto);

                // Xử lý ImageTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.ImageTempKey))
                {
                    if (_minioFileStorage != null)
                    {
                        var imageResult = await _minioFileStorage.CommitFileAsync(
                            createFlashCardDto.ImageTempKey,
                            IMAGE_BUCKET_NAME,
                            FlashCardFolder
                        );

                        if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                        {
                            _logger.LogError("Failed to commit image: {Error}", imageResult.Message);
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = $"Không thể lưu ảnh: {imageResult.Message}";
                            return response;
                        }

                        flashCard.ImageKey = imageResult.Data;
                    }
                    else
                    {
                        flashCard.ImageKey = createFlashCardDto.ImageTempKey;
                    }
                }

                // Xử lý AudioTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.AudioTempKey))
                {
                    if (_minioFileStorage != null)
                    {
                        var audioResult = await _minioFileStorage.CommitFileAsync(
                            createFlashCardDto.AudioTempKey,
                            AUDIO_BUCKET_NAME,
                            FlashCardFolder
                        );

                        if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                        {
                            _logger.LogError("Failed to commit audio: {Error}", audioResult.Message);
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = $"Không thể lưu audio: {audioResult.Message}";
                            return response;
                        }

                        flashCard.AudioKey = audioResult.Data;
                    }
                    else
                    {
                        flashCard.AudioKey = createFlashCardDto.AudioTempKey;
                    }
                }

                // Save to database
                FlashCard createdFlashCard;
                try
                {
                    createdFlashCard = await _flashCardRepository.CreateAsync(flashCard);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating flashcard");

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo flashcard";
                    return response;
                }
                
                var flashCardDto = _mapper.Map<FlashCardDto>(createdFlashCard);

                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCardDto.AudioUrl);
                }

                response.Data = flashCardDto;
                response.Message = "Tạo FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher tạo flash card: {CardFront}", createFlashCardDto.Word);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi tạo flash card";
            }

            return response;
        }

        // Teacher tạo nhiều flash card từ file Excel
        public async Task<ServiceResponse<List<FlashCardDto>>> TeacherBulkCreateFlashCards(BulkImportFlashCardDto bulkImportDto, int teacherId)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();

            try
            {
                // RLS đã filter modules theo TeacherId (chỉ modules của teacher này)
                var module = await _moduleRepository.GetByIdAsync(bulkImportDto.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    response.Data = new List<FlashCardDto>();
                    return response;
                }

                // Defense in depth: Check ownership
                var lesson = await _lessonRepository.GetLessonById(module.LessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    response.Data = new List<FlashCardDto>();
                    return response;
                }

                var course = await _courseRepository.GetCourseById(lesson.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    response.Data = new List<FlashCardDto>();
                    return response;
                }

                if (!course.TeacherId.HasValue || course.TeacherId.Value != teacherId)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền tạo flash card trong module này";
                    response.Data = new List<FlashCardDto>();
                    _logger.LogWarning("Teacher {TeacherId} attempted to bulk create flashcards in module {ModuleId}, course {CourseId} owned by {OwnerId}",
                        teacherId, bulkImportDto.ModuleId, lesson.CourseId, course.TeacherId);
                    return response;
                }

                // Map DTOs to entities
                var entities = bulkImportDto.FlashCards.Select(dto =>
                {
                    var fc = _mapper.Map<FlashCard>(dto);
                    fc.ModuleId = bulkImportDto.ModuleId;
                    return fc;
                }).ToList();

                // Create bulk
                var created = await _flashCardRepository.CreateBulkAsync(entities);
                var createdDtos = _mapper.Map<List<FlashCardDto>>(created);

                // Generate URLs
                foreach (var dto in createdDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.ImageUrl)) dto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, dto.ImageUrl);
                    if (!string.IsNullOrWhiteSpace(dto.AudioUrl)) dto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, dto.AudioUrl);
                }

                response.Data = createdDtos;
                response.Message = "Import flashcards thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher import flashcards hàng loạt");
                response.Success = false;
                response.Message = "Có lỗi khi import flashcards";
            }

            return response;
        }

        // Teacher cập nhật flashcard (RLS đã filter theo ownership)
        public async Task<ServiceResponse<FlashCardDto>> UpdateFlashCard(int flashCardId, UpdateFlashCardDto updateFlashCardDto)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                var existingFlashCard = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (existingFlashCard == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
                }

                // Cập nhật các trường được gửi lên
                _mapper.Map(updateFlashCardDto, existingFlashCard);
                string? newImageKey = null;
                string? newAudioKey = null;
                string? oldImageKey = existingFlashCard.ImageKey;
                string? oldAudioKey = existingFlashCard.AudioKey;

                // Xử lý cập nhật ImageUrl
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.ImageTempKey))
                {
                    if (_minioFileStorage != null)
                    {
                        var imageResult = await _minioFileStorage.CommitFileAsync(
                            updateFlashCardDto.ImageTempKey,
                            IMAGE_BUCKET_NAME,
                            FlashCardFolder
                        );

                        if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                        {
                            _logger.LogError("Failed to commit new image: {Error}", imageResult.Message);
                            response.Success = false;
                            response.Message = $"Không thể lưu ảnh mới: {imageResult.Message}";
                            return response;
                        }

                        newImageKey = imageResult.Data;
                        existingFlashCard.ImageKey = newImageKey;
                    }
                    else
                    {
                        existingFlashCard.ImageKey = updateFlashCardDto.ImageTempKey;
                    }
                }

                // Xử lý cập nhật AudioUrl
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.AudioTempKey))
                {
                    if (_minioFileStorage != null)
                    {
                        var audioResult = await _minioFileStorage.CommitFileAsync(
                            updateFlashCardDto.AudioTempKey,
                            AUDIO_BUCKET_NAME,
                            FlashCardFolder
                        );

                        if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                        {
                            _logger.LogError("Failed to commit new audio: {Error}", audioResult.Message);

                            // Rollback new image if committed
                            if (newImageKey != null && _minioFileStorage != null)
                            {
                                await _minioFileStorage.DeleteFileAsync(newImageKey, IMAGE_BUCKET_NAME);
                                existingFlashCard.ImageKey = oldImageKey;
                            }

                            response.Success = false;
                            response.Message = $"Không thể lưu audio mới: {audioResult.Message}";
                            return response;
                        }

                        newAudioKey = audioResult.Data;
                        existingFlashCard.AudioKey = newAudioKey;
                    }
                    else
                    {
                        existingFlashCard.AudioKey = updateFlashCardDto.AudioTempKey;
                    }
                }

                // Update database with rollback on failure
                FlashCard updatedFlashCard;
                try
                {
                    updatedFlashCard = await _flashCardRepository.UpdateAsync(existingFlashCard);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating flashcard");

                    // Rollback new files if MinIO configured
                    if (newImageKey != null && _minioFileStorage != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newImageKey, IMAGE_BUCKET_NAME);
                    }
                    if (newAudioKey != null && _minioFileStorage != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newAudioKey, AUDIO_BUCKET_NAME);
                    }

                    response.Success = false;
                    response.Message = "Lỗi database khi cập nhật flashcard";
                    return response;
                }

                // Only delete old files after successful DB update
                if (_minioFileStorage != null)
                {
                    if (newImageKey != null && !string.IsNullOrWhiteSpace(oldImageKey))
                    {
                        await _minioFileStorage.DeleteFileAsync(oldImageKey, IMAGE_BUCKET_NAME);
                    }
                    if (newAudioKey != null && !string.IsNullOrWhiteSpace(oldAudioKey))
                    {
                        await _minioFileStorage.DeleteFileAsync(oldAudioKey, AUDIO_BUCKET_NAME);
                    }
                }

                var flashCardDto = _mapper.Map<FlashCardDto>(updatedFlashCard);

                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCardDto.AudioUrl);
                }

                response.Data = flashCardDto;
                response.Message = "Cập nhật FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật FlashCard với ID: {FlashCardId}", flashCardId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi cập nhật FlashCard";
            }

            return response;
        }

        // Teacher xóa flashcard (RLS đã filter theo ownership)
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
                    response.Message = "Không tìm thấy FlashCard hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    return response;
                }

                var oldImageKey = flashCard.ImageKey;
                var oldAudioKey = flashCard.AudioKey;

                // Delete from repository
                var deleted = false;
                try
                {
                    deleted = await _flashCardRepository.DeleteAsync(flashCardId);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while deleting flashcard: {FlashCardId}", flashCardId);
                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi xóa flashcard";
                    response.Data = false;
                    return response;
                }

                if (!deleted)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Không thể xóa FlashCard";
                    response.Data = false;
                    return response;
                }

                // Try to delete files from MinIO (best-effort)
                if (_minioFileStorage != null)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(oldImageKey))
                        {
                            await _minioFileStorage.DeleteFileAsync(oldImageKey, IMAGE_BUCKET_NAME);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete image from storage: {Key}", oldImageKey);
                    }

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(oldAudioKey))
                        {
                            await _minioFileStorage.DeleteFileAsync(oldAudioKey, AUDIO_BUCKET_NAME);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete audio from storage: {Key}", oldAudioKey);
                    }
                }

                response.Data = true;
                response.Message = "Xóa FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa FlashCard với ID: {FlashCardId}", flashCardId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi xóa FlashCard";
                response.Data = false;
            }

            return response;
        }

        // Teacher lấy flashcard theo ID (read-only)
        public async Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                var flashCard = await _flashCardRepository.GetByIdWithDetailsAsync(flashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
                }

                var flashCardDto = _mapper.Map<FlashCardDto>(flashCard);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCardDto.AudioUrl);
                }

                response.Data = flashCardDto;
                response.Message = "Lấy thông tin FlashCard thành công";
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy FlashCard với ID: {FlashCardId}", flashCardId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy thông tin FlashCard";
            }

            return response;
        }

        // Teacher lấy danh sách flashcard theo module (read-only)
        public async Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId)
        {
            var response = new ServiceResponse<List<ListFlashCardDto>>();

            try
            {
                var flashCards = await _flashCardRepository.GetByModuleIdWithDetailsAsync(moduleId);
                var flashCardDtos = _mapper.Map<List<ListFlashCardDto>>(flashCards);

                // Generate URLs cho tất cả flashcards
                foreach (var dto in flashCardDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, dto.ImageUrl);
                    }
                    if (!string.IsNullOrWhiteSpace(dto.AudioUrl))
                    {
                        dto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, dto.AudioUrl);
                    }
                }

                response.Data = flashCardDtos;
                response.Message = $"Lấy danh sách {flashCards.Count} FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách FlashCard theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi lấy danh sách FlashCard";
            }

            return response;
        }
    }
}
