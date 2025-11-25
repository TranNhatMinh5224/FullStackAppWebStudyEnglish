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
        private readonly IMapper _mapper;
        private readonly ILogger<FlashCardService> _logger;
        private readonly IMinioFileStorage _minioFileStorage;
        
        // MinIO bucket constants
        private const string FlashCardBucket = "flashcards";
        private const string FlashCardFolder = "real";

        public FlashCardService(
            IFlashCardRepository flashCardRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<FlashCardService> logger,
            IMinioFileStorage minioFileStorage)
        {
            _flashCardRepository = flashCardRepository;
            _unitOfWork = unitOfWork;
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
                if (flashCard?.Module?.Lesson?.Course == null) return false;

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
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(FlashCardBucket, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(FlashCardBucket, flashCardDto.AudioUrl);
                }
                
                // Tính toán thống kê review nếu có userId
                if (userId.HasValue && flashCard.Reviews.Any())
                {
                    var userReviews = flashCard.Reviews.Where(r => r.UserId == userId.Value).ToList();
                    flashCardDto.ReviewCount = userReviews.Count;
                    flashCardDto.SuccessRate = userReviews.Count > 0 ? 
                        (decimal)userReviews.Count(r => r.Quality >= 2) / userReviews.Count * 100 : 0;
                    flashCardDto.LastReviewedAt = userReviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault()?.ReviewedAt;
                    
                    var latestReview = userReviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
                    flashCardDto.CurrentLevel = latestReview?.RepetitionCount ?? 0;
                    flashCardDto.NextReviewAt = latestReview?.NextReviewDate;
                }

                response.Data = flashCardDto;
                response.Message = "Lấy thông tin FlashCard thành công";
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

                // Tính toán thống kê cho từng flashcard nếu có userId
                if (userId.HasValue)
                {
                    foreach (var dto in flashCardDtos)
                    {
                        var flashCard = flashCards.First(fc => fc.FlashCardId == dto.FlashCardId);
                        var userReviews = flashCard.Reviews.Where(r => r.UserId == userId.Value).ToList();
                        
                        dto.ReviewCount = userReviews.Count;
                        dto.SuccessRate = userReviews.Count > 0 ? 
                            (decimal)userReviews.Count(r => r.Quality >= 2) / userReviews.Count * 100 : 0;
                        
                        var latestReview = userReviews.OrderByDescending(r => r.ReviewedAt).FirstOrDefault();
                        dto.CurrentLevel = latestReview?.RepetitionCount ?? 0;
                    }
                }
                
                // Generate URLs cho tất cả flashcards
                foreach (var dto in flashCardDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(FlashCardBucket, dto.ImageUrl);
                    }
                    if (!string.IsNullOrWhiteSpace(dto.AudioUrl))
                    {
                        dto.AudioUrl = BuildPublicUrl.BuildURL(FlashCardBucket, dto.AudioUrl);
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

        // + Tạo flashcard mới
        public async Task<ServiceResponse<FlashCardDto>> CreateFlashCardAsync(CreateFlashCardDto createFlashCardDto, int createdByUserId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                // Validate dữ liệu
                var validation = await ValidateFlashCardDataAsync(createFlashCardDto);
                if (!validation.Success)
                {
                    response.Success = false;
                    response.Message = validation.Message;
                    return response;
                }

                // Kiểm tra từ đã tồn tại chưa
                if (createFlashCardDto.ModuleId.HasValue)
                {
                    var wordExists = await _flashCardRepository.WordExistsInModuleAsync(
                        createFlashCardDto.Word, createFlashCardDto.ModuleId.Value);
                    
                    if (wordExists)
                    {
                        response.Success = false;
                        response.Message = $"Từ '{createFlashCardDto.Word}' đã tồn tại trong module này";
                        return response;
                    }
                }

                var flashCard = _mapper.Map<FlashCard>(createFlashCardDto);
                string? committedImageKey = null;
                string? committedAudioKey = null;
                
                // Commit ImageTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.ImageTempKey))
                {
                    var imageResult = await _minioFileStorage.CommitFileAsync(
                        createFlashCardDto.ImageTempKey,
                        FlashCardBucket,
                        FlashCardFolder
                    );
                    
                    if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                    {
                        _logger.LogError("Failed to commit image: {Error}", imageResult.Message);
                        response.Success = false;
                        response.Message = $"Không thể lưu ảnh: {imageResult.Message}";
                        return response;
                    }
                    
                    committedImageKey = imageResult.Data;
                    flashCard.ImageUrl = committedImageKey;
                }
                
                // Commit AudioTempKey nếu có
                if (!string.IsNullOrWhiteSpace(createFlashCardDto.AudioTempKey))
                {
                    var audioResult = await _minioFileStorage.CommitFileAsync(
                        createFlashCardDto.AudioTempKey,
                        FlashCardBucket,
                        FlashCardFolder
                    );
                    
                    if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                    {
                        _logger.LogError("Failed to commit audio: {Error}", audioResult.Message);
                        
                        // Rollback image nếu đã commit
                        if (committedImageKey != null)
                        {
                            await _minioFileStorage.DeleteFileAsync(committedImageKey, FlashCardBucket);
                        }
                        
                        response.Success = false;
                        response.Message = $"Không thể lưu audio: {audioResult.Message}";
                        return response;
                    }
                    
                    committedAudioKey = audioResult.Data;
                    flashCard.AudioUrl = committedAudioKey;
                }
                
                // Save to database with rollback on failure
                FlashCard createdFlashCard;
                try
                {
                    createdFlashCard = await _flashCardRepository.CreateAsync(flashCard);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating flashcard");
                    
                    // Rollback MinIO files
                    if (committedImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, FlashCardBucket);
                    }
                    if (committedAudioKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedAudioKey, FlashCardBucket);
                    }
                    
                    response.Success = false;
                    response.Message = "Lỗi database khi tạo flashcard";
                    return response;
                }
                var flashCardDto = _mapper.Map<FlashCardDto>(createdFlashCard);
                
                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(FlashCardBucket, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(FlashCardBucket, flashCardDto.AudioUrl);
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

        // + Cập nhật flashcard
        public async Task<ServiceResponse<FlashCardDto>> UpdateFlashCardAsync(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int updatedByUserId)
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

                // Kiểm tra từ trùng lặp nếu có thay đổi từ
                if (!string.IsNullOrEmpty(updateFlashCardDto.Word) && 
                    updateFlashCardDto.Word != existingFlashCard.Word &&
                    existingFlashCard.ModuleId.HasValue)
                {
                    var wordExists = await _flashCardRepository.WordExistsInModuleAsync(
                        updateFlashCardDto.Word, existingFlashCard.ModuleId.Value, flashCardId);
                    
                    if (wordExists)
                    {
                        response.Success = false;
                        response.Message = $"Từ '{updateFlashCardDto.Word}' đã tồn tại trong module này";
                        return response;
                    }
                }

                // Cập nhật các trường được gửi lên
                _mapper.Map(updateFlashCardDto, existingFlashCard);
                string? newImageKey = null;
                string? newAudioKey = null;
                string? oldImageKey = existingFlashCard.ImageUrl;
                string? oldAudioKey = existingFlashCard.AudioUrl;
                
                // Xử lý cập nhật ImageUrl
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.ImageTempKey))
                {
                    var imageResult = await _minioFileStorage.CommitFileAsync(
                        updateFlashCardDto.ImageTempKey,
                        FlashCardBucket,
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
                    existingFlashCard.ImageUrl = newImageKey;
                }
                
                // Xử lý cập nhật AudioUrl
                if (!string.IsNullOrWhiteSpace(updateFlashCardDto.AudioTempKey))
                {
                    var audioResult = await _minioFileStorage.CommitFileAsync(
                        updateFlashCardDto.AudioTempKey,
                        FlashCardBucket,
                        FlashCardFolder
                    );
                    
                    if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                    {
                        _logger.LogError("Failed to commit new audio: {Error}", audioResult.Message);
                        
                        // Rollback new image if committed
                        if (newImageKey != null)
                        {
                            await _minioFileStorage.DeleteFileAsync(newImageKey, FlashCardBucket);
                            existingFlashCard.ImageUrl = oldImageKey; // Restore old
                        }
                        
                        response.Success = false;
                        response.Message = $"Không thể lưu audio mới: {audioResult.Message}";
                        return response;
                    }
                    
                    newAudioKey = audioResult.Data;
                    existingFlashCard.AudioUrl = newAudioKey;
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
                    
                    // Rollback new files
                    if (newImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newImageKey, FlashCardBucket);
                    }
                    if (newAudioKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(newAudioKey, FlashCardBucket);
                    }
                    
                    response.Success = false;
                    response.Message = "Lỗi database khi cập nhật flashcard";
                    return response;
                }
                
                // Only delete old files after successful DB update
                if (newImageKey != null && !string.IsNullOrWhiteSpace(oldImageKey))
                {
                    await _minioFileStorage.DeleteFileAsync(oldImageKey, FlashCardBucket);
                }
                if (newAudioKey != null && !string.IsNullOrWhiteSpace(oldAudioKey))
                {
                    await _minioFileStorage.DeleteFileAsync(oldAudioKey, FlashCardBucket);
                }
                var flashCardDto = _mapper.Map<FlashCardDto>(updatedFlashCard);
                
                // Generate URLs cho response
                if (!string.IsNullOrWhiteSpace(flashCardDto.ImageUrl))
                {
                    flashCardDto.ImageUrl = BuildPublicUrl.BuildURL(FlashCardBucket, flashCardDto.ImageUrl);
                }
                if (!string.IsNullOrWhiteSpace(flashCardDto.AudioUrl))
                {
                    flashCardDto.AudioUrl = BuildPublicUrl.BuildURL(FlashCardBucket, flashCardDto.AudioUrl);
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

        // + Xóa flashcard
        public async Task<ServiceResponse<bool>> DeleteFlashCardAsync(int flashCardId, int deletedByUserId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var flashCard = await _flashCardRepository.GetByIdAsync(flashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
                }
                
                // Xóa image từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(flashCard.ImageUrl))
                {
                    await _minioFileStorage.DeleteFileAsync(FlashCardBucket, flashCard.ImageUrl);
                }
                
                // Xóa audio từ MinIO nếu có
                if (!string.IsNullOrWhiteSpace(flashCard.AudioUrl))
                {
                    await _minioFileStorage.DeleteFileAsync(FlashCardBucket, flashCard.AudioUrl);
                }

                var deleted = await _flashCardRepository.DeleteAsync(flashCardId);
                response.Data = deleted;
                response.Message = deleted ? "Xóa FlashCard thành công" : "Không thể xóa FlashCard";
                response.Success = deleted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa FlashCard với ID: {FlashCardId}", flashCardId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa FlashCard";
            }

            return response;
        }

        // + Cập nhật flashcard với authorization
        public async Task<ServiceResponse<FlashCardDto>> UpdateFlashCardWithAuthorizationAsync(int flashCardId, UpdateFlashCardDto updateFlashCardDto, int userId, string userRole)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                // Admin có thể cập nhật tất cả
                if (userRole == "Admin")
                {
                    return await UpdateFlashCardAsync(flashCardId, updateFlashCardDto, userId);
                }

                // Teacher chỉ có thể cập nhật flashcard của mình
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherFlashCardPermission(flashCardId, userId);
                    if (!hasPermission)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền cập nhật FlashCard này";
                        return response;
                    }

                    return await UpdateFlashCardAsync(flashCardId, updateFlashCardDto, userId);
                }

                response.Success = false;
                response.Message = "Bạn không có quyền cập nhật FlashCard";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật FlashCard với authorization: FlashCardId={FlashCardId}, UserId={UserId}, Role={UserRole}", flashCardId, userId, userRole);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi cập nhật FlashCard";
            }

            return response;
        }

        // + Xóa flashcard với authorization
        public async Task<ServiceResponse<bool>> DeleteFlashCardWithAuthorizationAsync(int flashCardId, int userId, string userRole)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Admin có thể xóa tất cả
                if (userRole == "Admin")
                {
                    return await DeleteFlashCardAsync(flashCardId, userId);
                }

                // Teacher chỉ có thể xóa flashcard của mình
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherFlashCardPermission(flashCardId, userId);
                    if (!hasPermission)
                    {
                        response.Success = false;
                        response.Message = "Bạn không có quyền xóa FlashCard này";
                        return response;
                    }

                    return await DeleteFlashCardAsync(flashCardId, userId);
                }

                response.Success = false;
                response.Message = "Bạn không có quyền xóa FlashCard";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa FlashCard với authorization: FlashCardId={FlashCardId}, UserId={UserId}, Role={UserRole}", flashCardId, userId, userRole);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi xóa FlashCard";
            }

            return response;
        }

        // + Tìm kiếm flashcard
        public async Task<ServiceResponse<List<ListFlashCardDto>>> SearchFlashCardsAsync(string searchTerm, int? moduleId = null, int? userId = null)
        {
            var response = new ServiceResponse<List<ListFlashCardDto>>();

            try
            {
                var flashCards = await _flashCardRepository.SearchFlashCardsAsync(searchTerm, moduleId);
                var flashCardDtos = _mapper.Map<List<ListFlashCardDto>>(flashCards);

                response.Data = flashCardDtos;
                response.Message = $"Tìm thấy {flashCards.Count} FlashCard";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm FlashCard: {SearchTerm}", searchTerm);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tìm kiếm FlashCard";
            }

            return response;
        }



        // + Tạo nhiều flashcard cùng lúc với xử lý media files
        public async Task<ServiceResponse<List<FlashCardDto>>> CreateBulkFlashCardsAsync(BulkImportFlashCardDto bulkImportDto, int userId, string userRole)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();
            var committedFiles = new List<(string key, string type)>(); // Track for rollback

            try
            {
                // Validate có flashcards không
                if (bulkImportDto.FlashCards == null || !bulkImportDto.FlashCards.Any())
                {
                    response.Success = false;
                    response.Message = "Danh sách FlashCard không được để trống";
                    return response;
                }

                _logger.LogInformation("Starting bulk create for {Count} flashcards in module {ModuleId}", 
                    bulkImportDto.FlashCards.Count, bulkImportDto.ModuleId);

                // Validate từng flashcard
                foreach (var createDto in bulkImportDto.FlashCards)
                {
                    var validation = await ValidateFlashCardDataAsync(createDto);
                    if (!validation.Success)
                    {
                        response.Success = false;
                        response.Message = $"Flashcard '{createDto.Word}': {validation.Message}";
                        return response;
                    }
                }

                // Kiểm tra từ trùng lặp trong batch
                var duplicatesInBatch = bulkImportDto.FlashCards
                    .GroupBy(fc => fc.Word.ToLower())
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicatesInBatch.Any())
                {
                    response.Success = false;
                    response.Message = $"Có từ trùng lặp trong danh sách: {string.Join(", ", duplicatesInBatch)}";
                    return response;
                }

                // Kiểm tra từ đã tồn tại trong module
                var existingWords = await _flashCardRepository.GetByModuleIdWithDetailsAsync(bulkImportDto.ModuleId);
                var existingWordSet = existingWords.Select(fc => fc.Word.ToLower()).ToHashSet();
                var duplicatesInModule = bulkImportDto.FlashCards
                    .Where(fc => existingWordSet.Contains(fc.Word.ToLower()))
                    .Select(fc => fc.Word)
                    .ToList();

                if (duplicatesInModule.Any() && !bulkImportDto.ReplaceExisting)
                {
                    response.Success = false;
                    response.Message = $"Các từ sau đã tồn tại trong module: {string.Join(", ", duplicatesInModule)}. Sử dụng ReplaceExisting=true để ghi đè.";
                    return response;
                }

                // Phase 1: Commit tất cả media files (images + audios)
                var flashCards = new List<FlashCard>();
                for (int i = 0; i < bulkImportDto.FlashCards.Count; i++)
                {
                    var createDto = bulkImportDto.FlashCards[i];
                    var flashCard = _mapper.Map<FlashCard>(createDto);
                    flashCard.ModuleId = bulkImportDto.ModuleId;

                    try
                    {
                        // Commit Image nếu có
                        if (!string.IsNullOrWhiteSpace(createDto.ImageTempKey))
                        {
                            var imageResult = await _minioFileStorage.CommitFileAsync(
                                createDto.ImageTempKey,
                                FlashCardBucket,
                                FlashCardFolder
                            );

                            if (!imageResult.Success || string.IsNullOrWhiteSpace(imageResult.Data))
                            {
                                throw new Exception($"Không thể commit image cho từ '{createDto.Word}': {imageResult.Message}");
                            }

                            flashCard.ImageUrl = imageResult.Data;
                            committedFiles.Add((imageResult.Data, "image"));
                            _logger.LogInformation("Committed image for '{Word}': {Key}", createDto.Word, imageResult.Data);
                        }

                        // Commit Audio nếu có
                        if (!string.IsNullOrWhiteSpace(createDto.AudioTempKey))
                        {
                            var audioResult = await _minioFileStorage.CommitFileAsync(
                                createDto.AudioTempKey,
                                FlashCardBucket,
                                FlashCardFolder
                            );

                            if (!audioResult.Success || string.IsNullOrWhiteSpace(audioResult.Data))
                            {
                                throw new Exception($"Không thể commit audio cho từ '{createDto.Word}': {audioResult.Message}");
                            }

                            flashCard.AudioUrl = audioResult.Data;
                            committedFiles.Add((audioResult.Data, "audio"));
                            _logger.LogInformation("Committed audio for '{Word}': {Key}", createDto.Word, audioResult.Data);
                        }
                    }
                    catch (Exception commitEx)
                    {
                        _logger.LogError(commitEx, "Error committing files for flashcard {Index}: {Word}", i + 1, createDto.Word);
                        
                        // Rollback tất cả files đã commit
                        await RollbackCommittedFilesAsync(committedFiles);
                        
                        response.Success = false;
                        response.Message = $"Lỗi khi xử lý file cho từ '{createDto.Word}': {commitEx.Message}";
                        return response;
                    }

                    flashCards.Add(flashCard);
                }

                _logger.LogInformation("All media files committed successfully. Total: {Count} images, {AudioCount} audios", 
                    committedFiles.Count(f => f.type == "image"), 
                    committedFiles.Count(f => f.type == "audio"));

                // Phase 2: Bulk insert vào database với transaction
                List<FlashCard> createdFlashCards;
                try
                {
                    createdFlashCards = await _flashCardRepository.CreateBulkAsync(flashCards);
                    _logger.LogInformation("Bulk insert successful: {Count} flashcards created", createdFlashCards.Count);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error during bulk insert for module {ModuleId}", bulkImportDto.ModuleId);
                    
                    // Rollback tất cả files đã commit
                    await RollbackCommittedFilesAsync(committedFiles);
                    
                    response.Success = false;
                    response.Message = "Lỗi database khi lưu FlashCard hàng loạt. Tất cả thay đổi đã được hoàn tác.";
                    return response;
                }

                // Phase 3: Map to DTOs và generate public URLs
                var flashCardDtos = _mapper.Map<List<FlashCardDto>>(createdFlashCards);
                foreach (var dto in flashCardDtos)
                {
                    if (!string.IsNullOrWhiteSpace(dto.ImageUrl))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(FlashCardBucket, dto.ImageUrl);
                    }
                    if (!string.IsNullOrWhiteSpace(dto.AudioUrl))
                    {
                        dto.AudioUrl = BuildPublicUrl.BuildURL(FlashCardBucket, dto.AudioUrl);
                    }
                }

                response.Data = flashCardDtos;
                response.Message = $"Tạo thành công {createdFlashCards.Count} FlashCard cho Module {bulkImportDto.ModuleId}";
                
                _logger.LogInformation("Bulk create completed successfully: {Count} flashcards, {FileCount} files", 
                    createdFlashCards.Count, committedFiles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during bulk create for Module: {ModuleId}", bulkImportDto.ModuleId);
                
                // Rollback files nếu có lỗi
                await RollbackCommittedFilesAsync(committedFiles);
                
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo FlashCard hàng loạt";
            }

            return response;
        }

        // Helper method để rollback committed files
        private async Task RollbackCommittedFilesAsync(List<(string key, string type)> committedFiles)
        {
            if (!committedFiles.Any())
            {
                return;
            }

            _logger.LogWarning("Rolling back {Count} committed files", committedFiles.Count);
            
            foreach (var (key, type) in committedFiles)
            {
                try
                {
                    await _minioFileStorage.DeleteFileAsync(key, FlashCardBucket);
                    _logger.LogInformation("Rolled back {Type} file: {Key}", type, key);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback {Type} file: {Key}", type, key);
                }
            }
        }



        // + Validate dữ liệu flashcard
        public Task<ServiceResponse<bool>> ValidateFlashCardDataAsync(CreateFlashCardDto createDto)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(createDto.Word))
                {
                    response.Success = false;
                    response.Message = "Từ vựng không được để trống";
                    return Task.FromResult(response);
                }

                if (string.IsNullOrWhiteSpace(createDto.Meaning))
                {
                    response.Success = false;
                    response.Message = "Nghĩa của từ không được để trống";
                    return Task.FromResult(response);
                }

                // Validate word length
                if (createDto.Word.Length > 100)
                {
                    response.Success = false;
                    response.Message = "Từ vựng không được vượt quá 100 ký tự";
                    return Task.FromResult(response);
                }

                // Validate meaning length
                if (createDto.Meaning.Length > 500)
                {
                    response.Success = false;
                    response.Message = "Nghĩa của từ không được vượt quá 500 ký tự";
                    return Task.FromResult(response);
                }

                response.Data = true;
                response.Message = "Dữ liệu hợp lệ";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi validate FlashCard data");
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi kiểm tra dữ liệu";
            }

            return Task.FromResult(response);
        }

    }
}
