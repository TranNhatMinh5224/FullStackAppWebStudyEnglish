using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.FlashCard;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserFlashCardService : IUserFlashCardService
    {
        private readonly IFlashCardRepository _flashCardRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserFlashCardService> _logger;

        // MinIO bucket constants
        private const string AUDIO_BUCKET_NAME = "flashcard-audio";
        private const string IMAGE_BUCKET_NAME = "flashcards";

        public UserFlashCardService(
            IFlashCardRepository flashCardRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<UserFlashCardService> logger)
        {
            _flashCardRepository = flashCardRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Lấy thông tin flashcard (chỉ xem được nếu đã đăng ký course)
        public async Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int userId)
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

                // Check enrollment: user phải đăng ký course mới được xem flashcard
                var courseId = flashCard.Module?.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem flashcard này";
                    _logger.LogWarning("User {UserId} attempted to access flashcard {FlashCardId} without enrollment in course {CourseId}", 
                        userId, flashCardId, courseId.Value);
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

                response.Success = true;
                response.StatusCode = 200;
                response.Data = flashCardDto;
                response.Message = "Lấy thông tin FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy FlashCard với ID: {FlashCardId}", flashCardId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy thông tin FlashCard";
            }

            return response;
        }

        // Lấy danh sách flashcard theo module (chỉ xem được nếu đã đăng ký course)
        public async Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<List<ListFlashCardDto>>();

            try
            {
                // Lấy module để check course
                var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check enrollment: user phải đăng ký course mới được xem flashcard
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, userId);
                if (!isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần đăng ký khóa học để xem flashcard";
                    _logger.LogWarning("User {UserId} attempted to list flashcards of module {ModuleId} without enrollment in course {CourseId}", 
                        userId, moduleId, courseId.Value);
                    return response;
                }

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

                response.Success = true;
                response.StatusCode = 200;
                response.Data = flashCardDtos;
                response.Message = $"Lấy danh sách {flashCards.Count} FlashCard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách FlashCard theo ModuleId: {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy danh sách FlashCard";
            }

            return response;
        }
    }
}
