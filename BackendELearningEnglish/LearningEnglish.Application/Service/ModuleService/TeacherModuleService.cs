using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{

    public class TeacherModuleService : ITeacherModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeacherModuleService> _logger;
        private readonly ILessonRepository _lessonRepository;
        private readonly ICourseRepository _courseRepository;
        private readonly IModuleImageService _moduleImageService;

        public TeacherModuleService(
            IModuleRepository moduleRepository,
            IMapper mapper,
            ILogger<TeacherModuleService> logger,
            ILessonRepository lessonRepository,
            ICourseRepository courseRepository,
            IModuleImageService moduleImageService)
        {
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _logger = logger;
            _lessonRepository = lessonRepository;
            _courseRepository = courseRepository;
            _moduleImageService = moduleImageService;
        }

        // Teacher tạo module
        public async Task<ServiceResponse<ModuleDto>> TeacherCreateModule(CreateModuleDto dto, int teacherId)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                var lesson = await _lessonRepository.GetLessonByIdForTeacher(dto.LessonId, teacherId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc không có quyền";
                    return response;
                }

                // Validate ownership: teacher phải sở hữu course của lesson
                var course = await _courseRepository.GetCourseByIdForTeacher(lesson.CourseId, teacherId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn không có quyền tạo module cho bài học này";
                    return response;
                }

                // Business logic: Chỉ teacher course mới được tạo module
                if (course.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể tạo module cho khóa học của giáo viên";
                    return response;
                }

                if (dto.OrderIndex <= 0)
                {
                    dto.OrderIndex = await _moduleRepository.GetMaxOrderIndexAsync(dto.LessonId) + 1;
                }

                var module = _mapper.Map<Module>(dto);
                string? committedImageKey = null;

                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    try
                    {
                        committedImageKey = await _moduleImageService.CommitImageAsync(dto.ImageTempKey);
                        module.ImageKey = committedImageKey;
                        module.ImageType = dto.ImageType;
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit module image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh module. Vui lòng thử lại.";
                        return response;
                    }
                }

                var created = await _moduleRepository.CreateAsync(module);
                var fullModule = await _moduleRepository.GetByIdWithDetailsAsync(created.ModuleId);
                var resultDto = _mapper.Map<ModuleDto>(fullModule);

                if (!string.IsNullOrWhiteSpace(fullModule?.ImageKey))
                {
                    resultDto.ImageUrl = _moduleImageService.BuildImageUrl(fullModule.ImageKey);
                }

                response.Data = resultDto;
                response.StatusCode = 201;
                response.Message = "Tạo module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi teacher tạo module");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi tạo module";
                return response;
            }
        }

        // Teacher lấy module theo ID - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<ModuleDto>> GetModuleById(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<ModuleDto>();
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
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem module này";
                    _logger.LogWarning("Teacher {TeacherId} attempted to access module {ModuleId} without ownership or enrollment", 
                        teacherId, moduleId);
                    return response;
                }

                // Load full details
                var fullModule = await _moduleRepository.GetByIdWithDetailsAsync(moduleId);
                if (fullModule == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                var dto = _mapper.Map<ModuleDto>(fullModule);

                if (!string.IsNullOrWhiteSpace(fullModule.ImageKey))
                {
                    dto.ImageUrl = _moduleImageService.BuildImageUrl(fullModule.ImageKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = dto;
                response.Message = "Lấy thông tin module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy module {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy thông tin module";
                return response;
            }
        }

        // Teacher lấy danh sách module theo lesson - Teacher có thể xem nếu là owner HOẶC đã enroll
        public async Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonId(int lessonId, int teacherId)
        {
            var response = new ServiceResponse<List<ListModuleDto>>();
            try
            {
                // Lấy lesson để check course
                var lesson = await _lessonRepository.GetLessonById(lessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học";
                    return response;
                }

                // Check: teacher phải là owner HOẶC đã enroll
                var course = await _courseRepository.GetCourseById(lesson.CourseId);
                if (course == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy khóa học";
                    return response;
                }

                var isOwner = course.TeacherId.HasValue && course.TeacherId.Value == teacherId;
                var isEnrolled = await _courseRepository.IsUserEnrolled(lesson.CourseId, teacherId);

                if (!isOwner && !isEnrolled)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Bạn cần sở hữu hoặc đăng ký khóa học để xem các module";
                    _logger.LogWarning("Teacher {TeacherId} attempted to list modules of lesson {LessonId} without ownership or enrollment", 
                        teacherId, lessonId);
                    return response;
                }

                // Nếu là owner, dùng method filter theo teacherId. Nếu chỉ enroll, dùng method thường
                var modules = isOwner 
                    ? await _moduleRepository.GetByLessonIdForTeacherAsync(lessonId, teacherId)
                    : await _moduleRepository.GetByLessonIdAsync(lessonId);
                var dtos = _mapper.Map<List<ListModuleDto>>(modules);

                response.Success = true;
                response.StatusCode = 200;
                response.Data = dtos;
                response.Message = "Lấy danh sách module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách module cho lesson {LessonId}", lessonId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy danh sách module";
                return response;
            }
        }

        // Teacher cập nhật module (own course only)
        public async Task<ServiceResponse<ModuleDto>> UpdateModule(int moduleId, UpdateModuleDto dto, int teacherId)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                // Validate ownership: teacher phải sở hữu module qua course
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(moduleId, teacherId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update module {ModuleId} without ownership", 
                        teacherId, moduleId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được cập nhật module
                if (module.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể cập nhật module của khóa học giáo viên";
                    _logger.LogWarning("Teacher {TeacherId} attempted to update module {ModuleId} of System course", 
                        teacherId, moduleId);
                    return response;
                }

                // Load module để update (module đã được validate ownership ở trên)
                var moduleToUpdate = await _moduleRepository.GetByIdAsync(moduleId);
                if (moduleToUpdate == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                string? newImageKey = null;
                var oldImageKey = moduleToUpdate.ImageKey;

                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    try
                    {
                        newImageKey = await _moduleImageService.CommitImageAsync(dto.ImageTempKey);
                        moduleToUpdate.ImageKey = newImageKey;
                        moduleToUpdate.ImageType = dto.ImageType;
                    }
                    catch (Exception imageEx)
                    {
                        _logger.LogError(imageEx, "Failed to commit new module image");
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể cập nhật ảnh module. Vui lòng thử lại.";
                        return response;
                    }
                }

                _mapper.Map(dto, moduleToUpdate);
                var updated = await _moduleRepository.UpdateAsync(moduleToUpdate);

                // Xóa ảnh cũ chỉ sau khi update thành công
                if (!string.IsNullOrWhiteSpace(oldImageKey) && newImageKey != null)
                {
                    await _moduleImageService.DeleteImageAsync(oldImageKey);
                }

                var fullModule = await _moduleRepository.GetByIdWithDetailsAsync(updated.ModuleId);
                var resultDto = _mapper.Map<ModuleDto>(fullModule);

                if (!string.IsNullOrWhiteSpace(fullModule?.ImageKey))
                {
                    resultDto.ImageUrl = _moduleImageService.BuildImageUrl(fullModule.ImageKey);
                }

                response.Success = true;
                response.StatusCode = 200;
                response.Data = resultDto;
                response.Message = "Cập nhật module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật module {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi cập nhật module";
                return response;
            }
        }

        // Teacher xóa module (own course only)
        public async Task<ServiceResponse<bool>> DeleteModule(int moduleId, int teacherId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Validate ownership: teacher phải sở hữu module qua course
                var module = await _moduleRepository.GetModuleWithCourseForTeacherAsync(moduleId, teacherId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module hoặc bạn không có quyền truy cập";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete module {ModuleId} without ownership", 
                        teacherId, moduleId);
                    return response;
                }

                // Business logic: Chỉ teacher course mới được xóa module
                if (module.Lesson?.Course?.Type != CourseType.Teacher)
                {
                    response.Success = false;
                    response.StatusCode = 403;
                    response.Message = "Chỉ có thể xóa module của khóa học giáo viên";
                    response.Data = false;
                    _logger.LogWarning("Teacher {TeacherId} attempted to delete module {ModuleId} of System course", 
                        teacherId, moduleId);
                    return response;
                }

                // Xóa ảnh module trên MinIO nếu có
                if (!string.IsNullOrWhiteSpace(module.ImageKey))
                {
                    await _moduleImageService.DeleteImageAsync(module.ImageKey);
                }

                response.Data = await _moduleRepository.DeleteAsync(moduleId);
                response.Success = true;
                response.StatusCode = 200;
                response.Message = "Xóa module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa module {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xóa module";
                response.Data = false;
                return response;
            }
        }
    }
}

