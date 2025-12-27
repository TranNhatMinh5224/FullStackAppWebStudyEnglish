using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.FlashCard;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.FlashCardService
{
        public class TeacherFlashCardQueryService : ITeacherFlashCardQueryService
        {
            private readonly IFlashCardRepository _flashCardRepository;
            private readonly IModuleRepository _moduleRepository;
            private readonly ICourseRepository _courseRepository;
            private readonly IMapper _mapper;
            private readonly ILogger<TeacherFlashCardQueryService> _logger;

        // MinIO bucket constants
        private const string AUDIO_BUCKET_NAME = "flashcard-audio";
        private const string IMAGE_BUCKET_NAME = "flashcards";

        public TeacherFlashCardQueryService(
            IFlashCardRepository flashCardRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherFlashCardQueryService> logger)
        {
            _flashCardRepository = flashCardRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
        }

        // Teacher lấy flashcard theo ID - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<FlashCardDto>> GetFlashCardByIdAsync(int flashCardId, int teacherId)
        {
            var response = new ServiceResponse<FlashCardDto>();

            try
            {
                // Lấy flashcard với navigation properties để check
                var flashCard = await _flashCardRepository.GetByIdWithDetailsAsync(flashCardId);
                if (flashCard == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy flashcard";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var courseId = flashCard.Module?.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var course = await _courseRepository.GetCourseById(courseId.Value);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem flashcard này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to access flashcard {FlashCardId} without ownership or enrollment", 
                        teacherId, flashCardId);
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
                response.Message = "Lấy flashcard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy flashcard {FlashCardId} cho teacher {TeacherId}", flashCardId, teacherId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy flashcard";
            }

            return response;
        }

        // Teacher lấy danh sách flashcard theo module - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<List<ListFlashCardDto>>> GetFlashCardsByModuleIdAsync(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<List<ListFlashCardDto>>();

            try
            {
                // Lấy module với course để check
                var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var courseId = module.Lesson?.CourseId;
                if (!courseId.HasValue)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var course = await _courseRepository.GetCourseById(courseId.Value);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(courseId.Value, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem các flashcard";
                    _logger.LogWarning("Teacher {TeacherId} attempted to list flashcards of module {ModuleId} without ownership or enrollment", 
                        teacherId, moduleId);
                    return response;
                }

                // Nếu là owner, dùng method filter theo teacherId. Nếu chỉ enroll, dùng method thường
                var flashCards = isOwner 
                    ? await _flashCardRepository.GetByModuleIdForTeacherAsync(moduleId, teacherId)
                    : await _flashCardRepository.GetByModuleIdWithDetailsAsync(moduleId);

                var flashCardDtos = flashCards.Select(fc =>
                {
                    var dto = _mapper.Map<ListFlashCardDto>(fc);
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
                response.StatusCode = 200;
                response.Data = flashCardDtos;
                response.Message = "Lấy danh sách flashcard thành công";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách flashcard cho module {ModuleId} và teacher {TeacherId}", moduleId, teacherId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Có lỗi xảy ra khi lấy danh sách flashcard";
            }

            return response;
        }
    }
}