using AutoMapper;
using LearningEnglish.Application.Common;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Services.Module;
using LearningEnglish.Application.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Application.Service
{
    public class UserModuleService : IUserModuleService
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UserModuleService> _logger;
        private readonly IModuleCompletionRepository _moduleCompletionRepository;

        private const string ModuleImageBucket = "modules";
        private const string ModuleImageFolder = "real";

        public UserModuleService(
            IModuleRepository moduleRepository,
            IMapper mapper,
            ILogger<UserModuleService> logger,
            IModuleCompletionRepository moduleCompletionRepository)
        {
            _moduleRepository = moduleRepository;
            _mapper = mapper;
            _logger = logger;
            _moduleCompletionRepository = moduleCompletionRepository;
        }

        // Lấy module với tiến độ học tập
        public async Task<ServiceResponse<ModuleWithProgressDto>> GetModuleWithProgress(int moduleId, int userId)
        {
            var response = new ServiceResponse<ModuleWithProgressDto>();
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

                var dto = _mapper.Map<ModuleWithProgressDto>(module);

                if (!string.IsNullOrWhiteSpace(module.ImageKey))
                {
                    dto.ImageUrl = BuildPublicUrl.BuildURL(ModuleImageBucket, module.ImageKey);
                }

                var completion = await _moduleCompletionRepository
                    .GetByUserAndModuleAsync(userId, moduleId);

                if (completion != null)
                {
                    dto.IsCompleted = completion.IsCompleted;
                    dto.ProgressPercentage = completion.ProgressPercentage;
                    dto.StartedAt = completion.StartedAt;
                    dto.CompletedAt = completion.CompletedAt;
                }

                response.Data = dto;
                response.Message = "Lấy thông tin module với tiến độ thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy module với tiến độ {ModuleId}", moduleId);
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy module với tiến độ";
                return response;
            }
        }

        // Lấy danh sách module với tiến độ học tập
        public async Task<ServiceResponse<List<ModuleWithProgressDto>>> GetModulesWithProgress(int lessonId, int userId)
        {
            var response = new ServiceResponse<List<ModuleWithProgressDto>>();
            try
            {
                var modules = await _moduleRepository.GetByLessonIdWithDetailsAsync(lessonId);
                var completions = await _moduleCompletionRepository
                    .GetByUserAndModuleIdsAsync(userId, modules.Select(x => x.ModuleId).ToList());

                var result = new List<ModuleWithProgressDto>();

                foreach (var module in modules)
                {
                    var dto = _mapper.Map<ModuleWithProgressDto>(module);

                    if (!string.IsNullOrWhiteSpace(module.ImageKey))
                    {
                        dto.ImageUrl = BuildPublicUrl.BuildURL(ModuleImageBucket, module.ImageKey);
                    }

                    var completion = completions.FirstOrDefault(x => x.ModuleId == module.ModuleId);
                    if (completion != null)
                    {
                        dto.IsCompleted = completion.IsCompleted;
                        dto.ProgressPercentage = completion.ProgressPercentage;
                        dto.StartedAt = completion.StartedAt;
                        dto.CompletedAt = completion.CompletedAt;
                    }

                    result.Add(dto);
                }

                response.Data = result;
                response.Message = "Lấy danh sách module với tiến độ thành công";
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy module với tiến độ");
                response.Success = false;
                response.StatusCode = 500;
                response.Message = "Đã xảy ra lỗi khi lấy module với tiến độ";
                return response;
            }
        }
    }
}

