using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOS;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class ModuleService : IModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ModuleService> _logger;

        public ModuleService(
            IModuleRepository moduleRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<ModuleService> logger)
        {
            _moduleRepository = moduleRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        // + Kiểm tra quyền teacher với module
        public async Task<bool> CheckTeacherModulePermission(int moduleId, int teacherId)
        {
            var module = await _moduleRepository.GetModuleWithCourseAsync(moduleId);
            if (module == null || module.Lesson == null || module.Lesson.Course == null)
            {
                return false;
            }

            var course = module.Lesson.Course;
            // Kiểm tra khóa học thuộc loại Teacher và thuộc về teacher này
            if (course.Type != Domain.Enums.CourseType.Teacher || course.TeacherId != teacherId)
            {
                return false;
            }

            return true;
        }

        // + Lấy thông tin module theo ID
        public async Task<ServiceResponse<ModuleDto>> GetModuleByIdAsync(int moduleId, int? userId = null)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                // Lấy module với thông tin chi tiết từ database
                var module = await _moduleRepository.GetByIdWithDetailsAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Chuyển đổi entity sang DTO để trả về client
                var moduleDto = _mapper.Map<ModuleDto>(module);
                response.Data = moduleDto;
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

        // + Lấy danh sách module theo lesson
        public async Task<ServiceResponse<List<ListModuleDto>>> GetModulesByLessonIdAsync(int lessonId, int? userId = null)
        {
            var response = new ServiceResponse<List<ListModuleDto>>();
            try
            {
                // Lấy tất cả module thuộc lesson này
                var modules = await _moduleRepository.GetByLessonIdAsync(lessonId);
                // Chuyển đổi sang DTO để trả về (ListModuleDto chỉ chứa thông tin cơ bản)
                var moduleDtos = _mapper.Map<List<ListModuleDto>>(modules);

                response.Data = moduleDtos;
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

        // + Tạo module mới
        public async Task<ServiceResponse<ModuleDto>> CreateModuleAsync(CreateModuleDto createModuleDto, int createdByUserId)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                // Tự động đặt thứ tự nếu chưa có (đảm bảo module mới luôn ở cuối)
                if (createModuleDto.OrderIndex <= 0)
                {
                    var maxOrder = await _moduleRepository.GetMaxOrderIndexAsync(createModuleDto.LessonId);
                    createModuleDto.OrderIndex = maxOrder + 1;
                }

                // Chuyển đổi DTO thành entity để lưu vào database
                var module = _mapper.Map<Module>(createModuleDto);
                var createdModule = await _moduleRepository.CreateAsync(module);

                // Lấy lại module đã tạo với đầy đủ thông tin để trả về
                var moduleWithDetails = await _moduleRepository.GetByIdWithDetailsAsync(createdModule.ModuleId);
                var moduleDto = _mapper.Map<ModuleDto>(moduleWithDetails);

                response.Data = moduleDto;
                response.StatusCode = 201; // Created
                response.Message = "Tạo module thành công";

                _logger.LogInformation("Module {ModuleId} được tạo thành công bởi user {UserId}", 
                    createdModule.ModuleId, createdByUserId);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo module cho lesson {LessonId}", createModuleDto.LessonId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi tạo module";
                return response;
            }
        }

        // + Cập nhật module
        public async Task<ServiceResponse<ModuleDto>> UpdateModuleAsync(int moduleId, UpdateModuleDto updateModuleDto, int updatedByUserId)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                // Kiểm tra module có tồn tại không
                var existingModule = await _moduleRepository.GetByIdAsync(moduleId);
                if (existingModule == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Áp dụng các thay đổi từ DTO vào entity hiện tại
                _mapper.Map(updateModuleDto, existingModule);
                var updatedModule = await _moduleRepository.UpdateAsync(existingModule);

                // Lấy lại module đã cập nhật với đầy đủ thông tin để trả về
                var moduleWithDetails = await _moduleRepository.GetByIdWithDetailsAsync(updatedModule.ModuleId);
                var moduleDto = _mapper.Map<ModuleDto>(moduleWithDetails);

                response.Data = moduleDto;
                response.Message = "Cập nhật module thành công";

                _logger.LogInformation("Module {ModuleId} được cập nhật thành công bởi user {UserId}", 
                    moduleId, updatedByUserId);

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

        // + Xóa module
        public async Task<ServiceResponse<bool>> DeleteModuleAsync(int moduleId, int deletedByUserId)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Kiểm tra module có tồn tại không
                var module = await _moduleRepository.GetByIdAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Thực hiện xóa module
                var result = await _moduleRepository.DeleteAsync(moduleId);
                response.Data = result;
                response.Message = result ? "Xóa module thành công" : "Không thể xóa module";

                if (result)
                {
                    _logger.LogInformation("Module {ModuleId} được xóa thành công bởi user {UserId}", 
                        moduleId, deletedByUserId);
                }

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

        // + Cập nhật module với authorization
        public async Task<ServiceResponse<ModuleDto>> UpdateModuleWithAuthorizationAsync(int moduleId, UpdateModuleDto updateModuleDto, int userId, string userRole)
        {
            var response = new ServiceResponse<ModuleDto>();
            try
            {
                // Kiểm tra module có tồn tại không
                var moduleResponse = await GetModuleByIdAsync(moduleId);
                if (!moduleResponse.Success || moduleResponse.Data == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                // Admin có thể cập nhật bất kỳ module nào
                if (userRole == "Admin")
                {
                    _logger.LogInformation("Admin {UserId} đang cập nhật module {ModuleId}", userId, moduleId);
                    return await UpdateModuleAsync(moduleId, updateModuleDto, userId);
                }

                // Teacher chỉ có thể cập nhật module từ khóa học của mình
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherModulePermission(moduleId, userId);
                    if (!hasPermission)
                    {
                        _logger.LogWarning("Teacher {UserId} cố gắng cập nhật module {ModuleId} không có quyền", userId, moduleId);
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn chỉ có thể cập nhật module từ khóa học của mình";
                        return response;
                    }

                    _logger.LogInformation("Teacher {UserId} đang cập nhật module {ModuleId}", userId, moduleId);
                    return await UpdateModuleAsync(moduleId, updateModuleDto, userId);
                }

                // Các role khác không có quyền
                response.Success = false;
                response.StatusCode = 403;
                response.Message = "Không có quyền truy cập";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong UpdateModuleWithAuthorizationAsync cho module {ModuleId} bởi user {UserId}", moduleId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                return response;
            }
        }

        // + Xóa module với authorization
        public async Task<ServiceResponse<bool>> DeleteModuleWithAuthorizationAsync(int moduleId, int userId, string userRole)
        {
            var response = new ServiceResponse<bool>();
            try
            {
                // Kiểm tra module có tồn tại không
                var moduleResponse = await GetModuleByIdAsync(moduleId);
                if (!moduleResponse.Success || moduleResponse.Data == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    response.Data = false;
                    return response;
                }

                // Admin có thể xóa bất kỳ module nào
                if (userRole == "Admin")
                {
                    _logger.LogInformation("Admin {UserId} đang xóa module {ModuleId}", userId, moduleId);
                    return await DeleteModuleAsync(moduleId, userId);
                }

                // Teacher chỉ có thể xóa module từ khóa học của mình
                if (userRole == "Teacher")
                {
                    var hasPermission = await CheckTeacherModulePermission(moduleId, userId);
                    if (!hasPermission)
                    {
                        _logger.LogWarning("Teacher {UserId} cố gắng xóa module {ModuleId} không có quyền", userId, moduleId);
                        response.Success = false;
                        response.StatusCode = 403;
                        response.Message = "Bạn chỉ có thể xóa module từ khóa học của mình";
                        response.Data = false;
                        return response;
                    }

                    _logger.LogInformation("Teacher {UserId} đang xóa module {ModuleId}", userId, moduleId);
                    return await DeleteModuleAsync(moduleId, userId);
                }

                // Các role khác không có quyền
                response.Success = false;
                response.StatusCode = 403;
                response.Message = "Không có quyền truy cập";
                response.Data = false;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong DeleteModuleWithAuthorizationAsync cho module {ModuleId} bởi user {UserId}", moduleId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi hệ thống";
                response.Data = false;
                return response;
            }
        }

        // + Lấy danh sách module với tiến độ
        public async Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgressAsync(int lessonId, int userId)
        {
            var response = new ServiceResponse<List<ModuleWithProgressDto>>();
            try
            {
                // Lấy tất cả module thuộc lesson với thông tin liên quan
                var modules = await _moduleRepository.GetByLessonIdWithDetailsAsync(lessonId);
                var moduleWithProgressDtos = new List<ModuleWithProgressDto>();

                foreach (var module in modules)
                {
                    var dto = _mapper.Map<ModuleWithProgressDto>(module);
                    
                    // Lấy thông tin tiến độ học tập từ bảng ModuleCompletion
                    var completion = module.ModuleCompletions?.FirstOrDefault(mc => mc.UserId == userId);
                    if (completion != null)
                    {
                        // User đã có tiến độ học tập cho module này
                        dto.IsCompleted = completion.IsCompleted;
                        dto.ProgressPercentage = completion.ProgressPercentage;
                        dto.StartedAt = completion.StartedAt;
                        dto.CompletedAt = completion.CompletedAt;
                    }
                    else
                    {
                        // User chưa bắt đầu học module này
                        dto.IsCompleted = false;
                        dto.ProgressPercentage = 0;
                        dto.StartedAt = null;
                        dto.CompletedAt = null;
                    }

                    moduleWithProgressDtos.Add(dto);
                }

                response.Data = moduleWithProgressDtos;
                response.Message = "Lấy danh sách module với tiến độ thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách module với tiến độ cho lesson {LessonId}", lessonId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy danh sách module với tiến độ";
                return response;
            }
        }

        // + Lấy module với tiến độ
        public async Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgressAsync(int moduleId, int userId)
        {
            var response = new ServiceResponse<ModuleWithProgressDto>();
            try
            {
                // Lấy module với thông tin chi tiết
                var module = await _moduleRepository.GetByIdWithDetailsAsync(moduleId);
                if (module == null)
                {
                    response.Success = false;
                    response.StatusCode = 404;
                    response.Message = "Không tìm thấy module";
                    return response;
                }

                var dto = _mapper.Map<ModuleWithProgressDto>(module);
                
                // Lấy thông tin tiến độ học tập cá nhân của user
                var completion = module.ModuleCompletions?.FirstOrDefault(mc => mc.UserId == userId);
                if (completion != null)
                {
                    // User đã có tiến độ học tập cho module này
                    dto.IsCompleted = completion.IsCompleted;
                    dto.ProgressPercentage = completion.ProgressPercentage;
                    dto.StartedAt = completion.StartedAt;
                    dto.CompletedAt = completion.CompletedAt;
                }
                else
                {
                    // User chưa bắt đầu học module này - thiết lập giá trị mặc định
                    dto.IsCompleted = false;
                    dto.ProgressPercentage = 0;
                    dto.StartedAt = null;
                    dto.CompletedAt = null;
                }

                response.Data = dto;
                response.Message = "Lấy thông tin module với tiến độ thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy module với tiến độ {ModuleId} cho user {UserId}", moduleId, userId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy thông tin module với tiến độ";
                return response;
            }
        }
    }
}
