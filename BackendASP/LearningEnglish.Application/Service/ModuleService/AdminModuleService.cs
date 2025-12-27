using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Domain.Entities;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class AdminModuleService : IAdminModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AdminModuleService> _logger;
        private readonly ILessonRepository _lessonRepository;
        private readonly IMinioFileStorage _minioFileStorage;

        private const string ModuleImageBucket = "modules";
        private const string ModuleImageFolder = "real";

        public AdminModuleService(
            IModuleRepository moduleRepository,
            IMapper mapper,
            ILogger<AdminModuleService> logger,
            ILessonRepository lessonRepository,
            IMinioFileStorage minioFileStorage)
        {
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _logger = logger;
            _lessonRepository = lessonRepository;
            _minioFileStorage = minioFileStorage;
        }

        // Admin tạo module
        public async Task<ServiceResponse<ModuleDto>> AdminCreateModule(CreateModuleDto dto)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                var lesson = await _lessonRepository.GetLessonById(dto.LessonId);
                if (lesson == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy bài học hoặc không có quyền";
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
                    var commit = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey,
                        ModuleImageBucket,
                        ModuleImageFolder
                    );

                    if (!commit.Success || string.IsNullOrWhiteSpace(commit.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể lưu ảnh module";
                        return response;
                    }

                    committedImageKey = commit.Data;
                    module.ImageKey = committedImageKey;
                    module.ImageType = dto.ImageType;
                }

                Module created;
                try
                {
                    created = await _moduleRepository.CreateAsync(module);
                }
                catch
                {
                    if (committedImageKey != null)
                    {
                        await _minioFileStorage.DeleteFileAsync(committedImageKey, ModuleImageBucket);
                    }
                    throw;
                }

                var fullModule = await _moduleRepository.GetByIdWithDetailsAsync(created.ModuleId);
                var resultDto = _mapper.Map<ModuleDto>(fullModule);

                if (!string.IsNullOrWhiteSpace(fullModule?.ImageKey))
                {
                    resultDto.ImageUrl = BuildPublicUrl.BuildURL(ModuleImageBucket, fullModule.ImageKey);
                }

                response.Data = resultDto;
                response.StatusCode = 201;
                response.Message = "Tạo module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi admin tạo module");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi tạo module";
                return response;
            }
        }

        // Admin lấy module theo ID
        public async Task<ServiceResponse<ModuleDto>> GetModuleById(int moduleId)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                var module = await _moduleRepository.GetByIdWithDetailsAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                var dto = _mapper.Map<ModuleDto>(module);

                if (!string.IsNullOrWhiteSpace(module.ImageKey))
                {
                    dto.ImageUrl = BuildPublicUrl.BuildURL(ModuleImageBucket, module.ImageKey);
                }

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

        // Admin lấy danh sách module theo lesson
        public async Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonId(int lessonId)
        {
            var response = new ServiceResponse<List<ListModuleDto>>();
            try
            {
                var modules = await _moduleRepository.GetByLessonIdAsync(lessonId);
                var dtos = _mapper.Map<List<ListModuleDto>>(modules);

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

        // Admin cập nhật module
        public async Task<ServiceResponse<ModuleDto>> UpdateModule(int moduleId, UpdateModuleDto dto)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                var module = await _moduleRepository.GetByIdAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                string? newImageKey = null;
                var oldImageKey = module.ImageKey;

                if (!string.IsNullOrWhiteSpace(dto.ImageTempKey))
                {
                    var commit = await _minioFileStorage.CommitFileAsync(
                        dto.ImageTempKey,
                        ModuleImageBucket,
                        ModuleImageFolder
                    );

                    if (!commit.Success || string.IsNullOrWhiteSpace(commit.Data))
                    {
                        response.Success = false;
                        response.StatusCode = 400;
                        response.Message = "Không thể cập nhật ảnh module";
                        return response;
                    }

                    newImageKey = commit.Data;
                    module.ImageKey = newImageKey;
                    module.ImageType = dto.ImageType;
                }

                _mapper.Map(dto, module);
                var updated = await _moduleRepository.UpdateAsync(module);

                if (!string.IsNullOrWhiteSpace(oldImageKey) && newImageKey != null)
                {
                    await _minioFileStorage.DeleteFileAsync(oldImageKey, ModuleImageBucket);
                }

                var fullModule = await _moduleRepository.GetByIdWithDetailsAsync(updated.ModuleId);
                var resultDto = _mapper.Map<ModuleDto>(fullModule);

                if (!string.IsNullOrWhiteSpace(fullModule?.ImageKey))
                {
                    resultDto.ImageUrl = BuildPublicUrl.BuildURL(ModuleImageBucket, fullModule.ImageKey);
                }

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

        // Admin xóa module
        public async Task<ServiceResponse<bool>> DeleteModule(int moduleId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                var module = await _moduleRepository.GetByIdAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                if (!string.IsNullOrWhiteSpace(module.ImageKey))
                {
                    await _minioFileStorage.DeleteFileAsync(module.ImageKey, ModuleImageBucket);
                }

                response.Data = await _moduleRepository.DeleteAsync(moduleId);
                response.Message = "Xóa module thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa module {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi xóa module";
                return response;
            }
        }
    }
}

