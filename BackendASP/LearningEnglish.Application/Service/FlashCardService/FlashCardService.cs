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
    public class FlashCardService : IFlashCardService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<FlashCardService> _logger;
        private readonly IMinioFileStorage? _minioFileStorage;

        // MinIO bucket constants
        private const string AUDIO_BUCKET_NAME = "flashcard-audio";
        private const string IMAGE_BUCKET_NAME = "flashcards";
        private const string FlashCardFolder = "real";


        public FlashCardService(
            IFlashCardRepository flashCardRepository,
            IUnitOfWork unitOfWork,
            IModuleRepository moduleRepository,
            IMapper mapper,
            ILogger<FlashCardService> logger,
            IMinioFileStorage? minioFileStorage = null)
        {
            _flashCardRepository = flashCardRepository;
            _unitOfWork = unitOfWork;
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _logger = logger;
            _minioFileStorage = minioFileStorage;
        }

        // + Kiểm tra quyền teacher với flashcard
        public async Task<bool> CheckTeacherFlashCardPermission(int flashCardId, int teacherId)
        {
            try
            {
                var flashCard = await _flashCardRepository.GetFlashCardWithModuleCourseAsync(flashCardId);
                if (flashCard?.Module?.Lesson?.Course == null) return false; // nếu flashcard không tồn tại  , hoặc không có module/lesson/course

                var course = flashCard.Module.Lesson.Course;
                return course.Type == CourseType.Teacher && course.TeacherId == teacherId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi kiểm tra quyền teacher với flashcard: {FlashCardId}, Teacher: {TeacherId}", flashCardId, teacherId);
                return false;
            }
        }

        // + Lấy thông tin flashcard theo ID
        public async Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int? userId = null)
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
                response.Data = flashCardDto;
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

        // + Lấy danh sách flashcard theo module
        public async Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int? userId = null)
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

        // + Lấy một flashcard theo chỉ số (phân trang từng card cho chế độ học)
        public async Task<ServiceResponse<PaginatedFlashCardDto>> GetFlashCardByIndexAsync(int moduleId, int cardIndex, int? userId = null)
        {
            var response = new ServiceResponse<PaginatedFlashCardDto>();

            try
            {
                // Validate cardIndex
                if (cardIndex < 1)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = "Chỉ số thẻ phải lớn hơn 0";
                    return response;
                }

                // Get total count
                var totalCards = await _flashCardRepository.GetFlashCardCountByModuleAsync(moduleId);

                if (totalCards == 0)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Module không có flashcard nào";
                    return response;
                }

                // Validate cardIndex against total
                if (cardIndex > totalCards)
                {
                    response.Success = false;
                    response.StatusCode = 400;
                    response.Message = $"Chỉ số thẻ vượt quá tổng số thẻ ({totalCards})";
                    return response;
                }

                // Get the single flashcard at the index
                var flashCard = await _flashCardRepository.GetSingleFlashCardByModuleAsync(moduleId, cardIndex);

                if (flashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard";
                    return response;
                }

                var flashCardDto = _mapper.Map<FlashCardDto>(flashCard);

                // Calculate review statistics from Reviews collection
                if (flashCard.Reviews != null && flashCard.Reviews.Any())
                {
                    var reviews = flashCard.Reviews.ToList();
                    flashCardDto.ReviewCount = reviews.Count;
                    
                    var successfulReviews = reviews.Count(r => r.Quality >= 3); // Quality >= 3 is considered successful
                    flashCardDto.SuccessRate = reviews.Count > 0 
                        ? Math.Round((decimal)successfulReviews / reviews.Count * 100, 2) 
                        : 0;
                    
                    var lastReview = reviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
                    if (lastReview != null)
                    {
                        flashCardDto.LastReviewedAt = lastReview.ReviewedAt;
                        flashCardDto.CurrentLevel = lastReview.RepetitionCount; // SRS level (số lần ôn thành công)
                        flashCardDto.NextReviewAt = lastReview.NextReviewDate;
                    }
                }
                else
                {
                    flashCardDto.ReviewCount = 0;
                    flashCardDto.SuccessRate = 0;
                    flashCardDto.CurrentLevel = 0;
                }

                // Generate URLs
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(IMAGE_BUCKET_NAME, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(AUDIO_BUCKET_NAME, flashCardDto.AudioUrl);
                }

                // Create paginated response
                response.Data = new PaginatedFlashCardDto
                {
                    FlashCard = flashCardDto,
                    CurrentIndex = cardIndex,
                    TotalCards = totalCards
                };

                response.Message = $"Lấy flashcard {cardIndex}/{totalCards} thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy flashcard index {CardIndex} theo ModuleId: {ModuleId}", cardIndex, moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy flashcard";
            }

            return response;
        }

        // + Tạo flashcard mới
        public async Task<ServiceResponse<FlashCardDto>> CreateFlashCardAsync(CreateFlashCardDto createFlashCardDto, int createdByUserId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
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
                _logger.LogError(ex, "Lỗi khi tạo FlashCard mới: {Word}", createFlashCardDto.Word);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo FlashCard";
            }

            return response;
        }


        // Core update logic (file-operations guarded if MinIO not configured)
        private async Task<ServiceResponse<FlashCardDto>> UpdateFlashCardCoreAsync(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int updatedByUserId)
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
                        // MinIO not configured: store temp key as placeholder
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
                                existingFlashCard.ImageKey = oldImageKey; // Restore old
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

                // Only delete old files after successful DB update (if MinIO configured)
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

        // + Cập nhật flashcard kèm xác thực quyền (Admin/Teacher)
        public async Task<ServiceResponse<FlashCardDto>> UpdateFlashCardAsync(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int userId, string userRole)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                if (userRole != "Admin" && userRole != "Teacher")
                {
                    response.Success = false;
                    response.Message = "Bạn không có quyền cập nhật FlashCard";
                    return response;
                }

                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherFlashCardPermission(flashCardId, userId);
                    if (!hasPermission)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền cập nhật FlashCard này";
                        return response;
                    }
                }

                // Delegate to core update method
                return await UpdateFlashCardCoreAsync(flashCardId, updateFlashCardDto, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xác thực và cập nhật FlashCard: {FlashCardId}", flashCardId);
                response.Success = false;
                response.Message = "Có lỗi khi cập nhật FlashCard";
            }

            return response;
        }

        // + Tạo flashcard hàng loạt
        public async Task<ServiceResponse<List<FlashCardDto>>> CreateBulkFlashCardsAsync(BulkImportFlashCardDto bulkImportDto, int userId, string userRole)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();

            try
            {
                if (userRole != "Admin" && userRole != "Teacher")
                {
                    response.Success = false;
                    response.Message = "Bạn không có quyền thực hiện thao tác này";
                    return response;
                }

                // If teacher, ensure they own the course that module belongs to
                if (userRole == "Teacher")
                {
                    var module = await _moduleRepository.GetModuleWithCourseAsync(bulkImportDto.ModuleId);
                    if (module?.Lesson?.Course == null || module.Lesson.Course.TeacherId != userId)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền import vào Module này";
                        return response;
                    }
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
                _logger.LogError(ex, "Lỗi khi import flashcards hàng loạt");
                response.Success = false;
                response.Message = "Có lỗi khi import flashcards";
            }

            return response;
        }

        // + Xóa flashcard
        public async Task<ServiceResponse<bool>> DeleteFlashCardAsync(int flashCardId, int userId, string userRole)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Authorization
                if (userRole != "Admin" && userRole != "Teacher")
                {
                    response.Success = false;
                    response.Message = "Bạn không có quyền xóa FlashCard";
                    return response;
                }

                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherFlashCardPermission(flashCardId, userId);
                    if (!hasPermission)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền xóa FlashCard này";
                        return response;
                    }
                }

                var flashCard = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashCard";
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
                    response.Message = "Lỗi database khi xóa flashcard";
                    return response;
                }

                if (!deleted)
                {
                    response.Success = false;
                    response.Message = "Không thể xóa FlashCard";
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
                        _logger.LogWarning(ex, "Failed to delete image from storage after deleting flashcard: {Key}", oldImageKey);
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
                        _logger.LogWarning(ex, "Failed to delete audio from storage after deleting flashcard: {Key}", oldAudioKey);
                    }
                }

                response.Data = true;
                response.Message = "Xóa FlashCard thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa FlashCard với ID: {FlashCardId}", flashCardId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa FlashCard";
            }

            return response;
        }
    }
}
