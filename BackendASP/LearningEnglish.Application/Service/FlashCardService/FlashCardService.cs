using AutoMapper;
using LearningEnglish.Application.Common;
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

        public FlashCardService(
            IFlashCardRepository flashCardRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<FlashCardService> logger)
        {
            _flashCardRepository = flashCardRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
                var createdFlashCard = await _flashCardRepository.CreateAsync(flashCard);
                var flashCardDto = _mapper.Map<FlashCardDto>(createdFlashCard);

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

                var updatedFlashCard = await _flashCardRepository.UpdateAsync(existingFlashCard);
                var flashCardDto = _mapper.Map<FlashCardDto>(updatedFlashCard);

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
                var exists = await _flashCardRepository.ExistsAsync(flashCardId);
                if (!exists)
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy FlashCard";
                    return response;
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



        // + Tạo nhiều flashcard cùng lúc
        public async Task<ServiceResponse<List<FlashCardDto>>> CreateBulkFlashCardsAsync(BulkImportFlashCardDto bulkImportDto, int userId, string userRole)
        {
            var response = new ServiceResponse<List<FlashCardDto>>();

            try
            {
                // Validate permissions for the target module
                // TODO: Add module permission check

                var flashCards = _mapper.Map<List<FlashCard>>(bulkImportDto.FlashCards);
                foreach (var fc in flashCards)
                {
                    fc.ModuleId = bulkImportDto.ModuleId;
                }

                var createdFlashCards = await _flashCardRepository.CreateBulkAsync(flashCards);
                var flashCardDtos = _mapper.Map<List<FlashCardDto>>(createdFlashCards);

                response.Data = flashCardDtos;
                response.Message = $"Tạo {createdFlashCards.Count} FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo bulk FlashCard cho Module: {ModuleId}", bulkImportDto.ModuleId);
                response.Success = false;
                response.Message = "Có lỗi xảy ra khi tạo FlashCard hàng loạt";
            }

            return response;
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
