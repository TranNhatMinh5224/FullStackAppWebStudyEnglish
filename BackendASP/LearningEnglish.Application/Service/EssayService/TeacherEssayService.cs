using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.Common.Constants;
using LearningEnglish.Application.Common.Helpers;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Essay;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service.EssayService
{
    
    public class TeacherEssayService : ITeacherEssayService
    {
        private readonly IEssayRepository _essayRepository;
        private readonly IAssessmentRepository _assessmentRepository;
        private readonly IModuleRepository _moduleRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherEssayService> _logger;
        private readonly IEssayMediaService _essayMediaService;

        public TeacherEssayService(
            IEssayRepository essayRepository,
            IAssessmentRepository assessmentRepository,
            IModuleRepository moduleRepository,
            ICourseRepository courseRepository,
            IMapper mapper,
            ILogger<TeacherEssayService> logger,
            IEssayMediaService essayMediaService)
        {
            _essayRepository = essayRepository;
            _assessmentRepository = assessmentRepository;
            _moduleRepository = moduleRepository;
            _courseRepository = courseRepository;
            _mapper = mapper;
            _logger = logger;
            _essayMediaService = essayMediaService;
        }

        public async Task<ServiceResponse<EssayDto>> TeacherCreateEssay(CreateEssayDto dto, int teacherId)
        {
            var response = new ServiceResponse<EssayDto>();

            try
            {
                // Kiểm tra Assessment có tồn tại không
                if (!await _essayRepository.AssessmentExistsAsync(dto.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Assessment không tồn tại";
                    return response;
                }

                // Kiểm tra quyền Teacher: assessment -> lesson -> course -> teacherId
                if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, dto.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền tạo Essay cho Assessment này";
                    return response;
                }

                // Map DTO to Entity
                var essay = new Essay
                {
                    AssessmentId = dto.AssessmentId,
                    Title = dto.Title,
                    Description = dto.Description,
                    Type = AssessmentType.Essay
                };

                string? committedAudioKey = null;
                string? committedImageKey = null;

                try
                {
                    // Commit Audio file nếu có
                    if (!string.IsNullOrWhiteSpace(dto.AudioTempKey))
                    {
                        try
                        {
                            committedAudioKey = await _essayMediaService.CommitAudioAsync(dto.AudioTempKey);
                            essay.AudioKey = committedAudioKey;
                            essay.AudioType = dto.AudioType;
                        }
                        catch (Exception audioEx)
                        {
                            _logger.LogError(audioEx, "Failed to commit essay audio");
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file audio. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Commit Image file nếu có
                    if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                    {
                        try
                        {
                            committedImageKey = await _essayMediaService.CommitImageAsync(dto.ImageTempKey);
                            essay.ImageKey = committedImageKey;
                            essay.ImageType = dto.ImageType;
                        }
                        catch (Exception imageEx)
                        {
                            _logger.LogError(imageEx, "Failed to commit essay image");
                            
                            // Rollback audio nếu đã commit
                            if (committedAudioKey != null)
                            {
                                await _essayMediaService.DeleteAudioAsync(committedAudioKey);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file hình ảnh. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Tạo Essay trong database
                    var createdEssay = await _essayRepository.CreateEssayAsync(essay);

                    // Map Entity to DTO
                    var essayDto = _mapper.Map<EssayDto>(createdEssay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(createdEssay.AudioKey))
                    {
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(createdEssay.AudioKey);
                        essayDto.AudioType = createdEssay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(createdEssay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(createdEssay.ImageKey);
                        essayDto.ImageType = createdEssay.ImageType;
                    }

                    response.Success = true;
                    response.StatusCode = 201;
                    response.Message = "Tạo Essay thành công";
                    response.Data = essayDto;

                    _logger.LogInformation("Teacher {TeacherId} created Essay {EssayId} with audio: {HasAudio}, image: {HasImage}",
                        teacherId,
                        createdEssay.EssayId,
                        !string.IsNullOrWhiteSpace(committedAudioKey),
                        !string.IsNullOrWhiteSpace(committedImageKey));

                    return response;
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while creating Essay");

                    // Rollback files if DB fails
                    if (committedAudioKey != null)
                    {
                        await _essayMediaService.DeleteAudioAsync(committedAudioKey);
                    }
                    if (committedImageKey != null)
                    {
                        await _essayMediaService.DeleteImageAsync(committedImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi tạo Essay";
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher {TeacherId} tạo Essay", teacherId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi tạo Essay";
                return response;
            }
        }

        // Teacher có thể xem Essay nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<EssayDto>> GetEssayByIdAsync(int essayId, int teacherId)
        {
            var response = new ServiceResponse<EssayDto>();

            try
            {
                var essay = await _essayRepository.GetEssayByIdWithDetailsAsync(essayId);

                if (essay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Lấy Assessment với Module và Course để check
                var assessment = await _assessmentRepository.GetAssessmentById(essay.AssessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Lấy Module với Course để check
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
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
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem Essay này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to access essay {EssayId} without ownership or enrollment", 
                        teacherId, essayId);
                    return response;
                }

                var essayDto = _mapper.Map<EssayDto>(essay);

                // Generate URLs từ keys
                if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                {
                    essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(essay.AudioKey);
                    essayDto.AudioType = essay.AudioType;
                }

                if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                {
                    essayDto.ImageUrl = _essayMediaService.BuildImageUrl(essay.ImageKey);
                    essayDto.ImageType = essay.ImageType;
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy thông tin Essay thành công";
                response.Data = essayDto;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher lấy thông tin Essay với ID {EssayId}", essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy thông tin Essay";
                return response;
            }
        }

        // Teacher có thể xem Essays nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<List<EssayDto>>> GetEssaysByAssessmentIdAsync(int assessmentId, int teacherId)
        {
            var response = new ServiceResponse<List<EssayDto>>();

            try
            {
                // Lấy Assessment với Module và Course để check
                var assessment = await _assessmentRepository.GetAssessmentById(assessmentId);
                if (assessment == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Assessment";
                    return response;
                }

                // Lấy Module với Course để check
                var module = await _moduleRepository.GetModuleWithCourseAsync(assessment.ModuleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy Module";
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
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem các Essay";
                    _logger.LogWarning("Teacher {TeacherId} attempted to list essays of assessment {AssessmentId} without ownership or enrollment", 
                        teacherId, assessmentId);
                    return response;
                }

                var essays = await _essayRepository.GetEssaysByAssessmentIdAsync(assessmentId);
                var essayDtos = new List<EssayDto>();

                foreach (var essay in essays)
                {
                    var essayDto = _mapper.Map<EssayDto>(essay);

                    // Generate URLs từ keys
                    if (!string.IsNullOrWhiteSpace(essay.AudioKey))
                    {
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(essay.AudioKey);
                        essayDto.AudioType = essay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(essay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(essay.ImageKey);
                        essayDto.ImageType = essay.ImageType;
                    }

                    essayDtos.Add(essayDto);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Lấy danh sách Essay thành công";
                response.Data = essayDtos;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher lấy danh sách Essay theo Assessment ID {AssessmentId}", assessmentId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi lấy danh sách Essay";
                return response;
            }
        }

        public async Task<ServiceResponse<EssayDto>> UpdateEssay(int essayId, UpdateEssayDto dto, int teacherId)
        {
            var response = new ServiceResponse<EssayDto>();

            try
            {
                var existingEssay = await _essayRepository.GetEssayByIdWithDetailsAsync(essayId);

                if (existingEssay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Kiểm tra quyền Teacher
                if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, existingEssay.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền cập nhật Essay này";
                    return response;
                }

                // Cập nhật thông tin cơ bản (nullable support)
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    existingEssay.Title = dto.Title;

                if (dto.Description != null)
                    existingEssay.Description = dto.Description;

                string? newAudioKey = null;
                string? newImageKey = null;
                string? oldAudioKey = !string.IsNullOrWhiteSpace(existingEssay.AudioKey) ? existingEssay.AudioKey : null;
                string? oldImageKey = !string.IsNullOrWhiteSpace(existingEssay.ImageKey) ? existingEssay.ImageKey : null;

                try
                {
                    // Xử lý Audio file mới
                    if (!string.IsNullOrWhiteSpace(dto.AudioTempKey))
                    {
                        try
                        {
                            newAudioKey = await _essayMediaService.CommitAudioAsync(dto.AudioTempKey);
                            existingEssay.AudioKey = newAudioKey;
                            existingEssay.AudioType = dto.AudioType;
                        }
                        catch (Exception audioEx)
                        {
                            _logger.LogError(audioEx, "Failed to commit new essay audio");
                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file audio mới. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Xử lý Image file mới
                    if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                    {
                        try
                        {
                            newImageKey = await _essayMediaService.CommitImageAsync(dto.ImageTempKey);
                            existingEssay.ImageKey = newImageKey;
                            existingEssay.ImageType = dto.ImageType;
                        }
                        catch (Exception imageEx)
                        {
                            _logger.LogError(imageEx, "Failed to commit new essay image");
                            
                            // Rollback audio nếu đã commit
                            if (newAudioKey != null && newAudioKey != oldAudioKey)
                            {
                                await _essayMediaService.DeleteAudioAsync(newAudioKey);
                            }

                            response.Success = false;
                            response.StatusCode = 400;
                            response.Message = "Không thể lưu file hình ảnh mới. Vui lòng thử lại.";
                            return response;
                        }
                    }

                    // Update database
                    var updatedEssay = await _essayRepository.UpdateEssayAsync(existingEssay);

                    // Xóa file cũ sau khi update thành công
                    if (newAudioKey != null && oldAudioKey != null && newAudioKey != oldAudioKey)
                    {
                        await _essayMediaService.DeleteAudioAsync(oldAudioKey);
                    }

                    if (newImageKey != null && oldImageKey != null && newImageKey != oldImageKey)
                    {
                        await _essayMediaService.DeleteImageAsync(oldImageKey);
                    }

                    // Map to DTO và generate URLs
                    var essayDto = _mapper.Map<EssayDto>(updatedEssay);

                    if (!string.IsNullOrWhiteSpace(updatedEssay.AudioKey))
                    {
                        essayDto.AudioUrl = _essayMediaService.BuildAudioUrl(updatedEssay.AudioKey);
                        essayDto.AudioType = updatedEssay.AudioType;
                    }

                    if (!string.IsNullOrWhiteSpace(updatedEssay.ImageKey))
                    {
                        essayDto.ImageUrl = _essayMediaService.BuildImageUrl(updatedEssay.ImageKey);
                        essayDto.ImageType = updatedEssay.ImageType;
                    }

                    response.Success = true;
                    response.StatusCode = 200;
                    response.Message = "Cập nhật Essay thành công";
                    response.Data = essayDto;

                    _logger.LogInformation("Teacher {TeacherId} updated Essay {EssayId}", teacherId, essayId);

                    return response;
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while updating Essay");

                    // Rollback new files if DB fails
                    if (newAudioKey != null && newAudioKey != oldAudioKey)
                    {
                        await _essayMediaService.DeleteAudioAsync(newAudioKey);
                    }
                    if (newImageKey != null && newImageKey != oldImageKey)
                    {
                        await _essayMediaService.DeleteImageAsync(newImageKey);
                    }

                    response.Success = false;
                    response.StatusCode = 500;
                    response.Message = "Lỗi database khi cập nhật Essay";
                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher {TeacherId} cập nhật Essay với ID {EssayId}", teacherId, essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi cập nhật Essay";
                return response;
            }
        }

        public async Task<ServiceResponse<bool>> DeleteEssay(int essayId, int teacherId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var existingEssay = await _essayRepository.GetEssayByIdWithDetailsAsync(essayId);

                if (existingEssay == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Essay không tồn tại";
                    return response;
                }

                // Kiểm tra quyền Teacher
                if (!await _essayRepository.IsTeacherOwnerOfAssessmentAsync(teacherId, existingEssay.AssessmentId))
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Teacher không có quyền xóa Essay này";
                    return response;
                }

                // Lưu keys trước khi xóa
                string? audioKey = existingEssay.AudioKey;
                string? imageKey = existingEssay.ImageKey;

                // Xóa files trong MinIO trước khi xóa database
                if (!string.IsNullOrWhiteSpace(audioKey))
                {
                    await _essayMediaService.DeleteAudioAsync(audioKey);
                }

                if (!string.IsNullOrWhiteSpace(imageKey))
                {
                    await _essayMediaService.DeleteImageAsync(imageKey);
                }

                // Xóa Essay trong database
                await _essayRepository.DeleteEssayAsync(essayId);

                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa Essay thành công";
                response.Data = true;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Teacher {TeacherId} xóa Essay với ID {EssayId}", teacherId, essayId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Lỗi hệ thống khi xóa Essay";
                return response;
            }
        }
    }
}
