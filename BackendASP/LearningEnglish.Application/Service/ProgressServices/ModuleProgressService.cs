using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LearningEnglish.Application.Service.ProgressServices
{
    public class ModuleProgressService : IModuleProgressService
    {
        private readonly IModuleCompletionRepository _moduleCompletionRepo;
        private readonly IModuleRepository _moduleRepo;
        private readonly ILessonCompletionRepository _lessonCompletionRepo;
        private readonly ILessonProgressService _lessonProgressService;
        private readonly ILogger<ModuleProgressService> _logger;

        public ModuleProgressService(
            IModuleCompletionRepository moduleCompletionRepo,
            IModuleRepository moduleRepo,
            ILessonCompletionRepository lessonCompletionRepo,
            ILessonProgressService lessonProgressService,
            ILogger<ModuleProgressService> logger)
        {
            _moduleCompletionRepo = moduleCompletionRepo;
            _moduleRepo = moduleRepo;
            _lessonCompletionRepo = lessonCompletionRepo;
            _lessonProgressService = lessonProgressService;
            _logger = logger;
        }

        public async Task<ServiceResponse<bool>> StartModuleAsync(int userId, int moduleId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var existingCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);

                if (existingCompletion != null)
                {
                    response.Success = true;
                    response.Data = true;
                    response.Message = $"Module {moduleId} already started for user {userId}";
                    _logger.LogInformation("Module {ModuleId} already started for user {UserId}", moduleId, userId);
                    return response;
                }

                var module = await _moduleRepo.GetByIdAsync(moduleId);

                if (module == null)
                {
                    response.Success = false;
                    response.Message = $"Module with ID {moduleId} not found";
                    return response;
                }

                // Create ModuleCompletion
                var moduleCompletion = new ModuleCompletion
                {
                    UserId = userId,
                    ModuleId = moduleId
                };
                moduleCompletion.MarkAsStarted();

                await _moduleCompletionRepo.AddAsync(moduleCompletion);

                // Create LessonCompletion if not exists
                var lessonId = await _moduleRepo.GetLessonIdByModuleIdAsync(moduleId);

                if (lessonId == null)
                {
                    response.Success = false;
                    response.Message = $"Could not find lesson for module {moduleId}";
                    return response;
                }

                var lessonCompletion = await _lessonCompletionRepo.GetByUserAndLessonAsync(userId, lessonId.Value);

                if (lessonCompletion == null)
                {
                    var allModules = await _moduleRepo.GetByLessonIdAsync(module.LessonId);

                    lessonCompletion = new LessonCompletion
                    {
                        UserId = userId,
                        LessonId = module.LessonId,
                        TotalModules = allModules.Count(),
                        StartedAt = DateTime.UtcNow
                    };

                    await _lessonCompletionRepo.AddAsync(lessonCompletion);
                }

                response.Success = true;
                response.Data = true;
                response.Message = "Module started successfully";

                _logger.LogInformation("Started module {ModuleId} for user {UserId}", moduleId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting module {ModuleId} for user {UserId}", moduleId, userId);
                response.Success = false;
                response.Message = $"Error starting module: {ex.Message}";
            }

            return response;
        }

        public async Task<ServiceResponse<bool>> CompleteModuleAsync(int userId, int moduleId)
        {
            var response = new ServiceResponse<bool>();

            try
            {
                var moduleCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);

                if (moduleCompletion == null)
                {
                    // If not started, start and complete it
                    await StartModuleAsync(userId, moduleId);
                    moduleCompletion = await _moduleCompletionRepo.GetByUserAndModuleAsync(userId, moduleId);
                }

                if (moduleCompletion != null && !moduleCompletion.IsCompleted)
                {
                    moduleCompletion.MarkAsCompleted();
                    await _moduleCompletionRepo.UpdateAsync(moduleCompletion);

                    // Update lesson progress
                    var lessonId = await _moduleRepo.GetLessonIdByModuleIdAsync(moduleId);

                    if (lessonId != null)
                    {
                        await _lessonProgressService.UpdateLessonProgressAsync(userId, lessonId.Value);
                    }

                    response.Success = true;
                    response.Data = true;
                    response.Message = "Module completed successfully";

                    _logger.LogInformation("Completed module {ModuleId} for user {UserId}", moduleId, userId);
                }
                else
                {
                    response.Success = true;
                    response.Data = true;
                    response.Message = "Module was already completed";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing module {ModuleId} for user {UserId}", moduleId, userId);
                response.Success = false;
                response.Message = $"Error completing module: {ex.Message}";
            }

            return response;
        }
    }
}