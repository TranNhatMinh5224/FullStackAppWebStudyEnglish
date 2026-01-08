using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.ImageService;

public class ModuleImageService : IModuleImageService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<ModuleImageService> _logger;

    public ModuleImageService(
        IMinioFileStorage minioFileStorage,
        ILogger<ModuleImageService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public async Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.ModuleImageBucket,
            StorageConstants.ModuleImageFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit module image. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit module image: {result.Message}");
        }

        _logger.LogInformation("Module image committed successfully. TempKey: {TempKey}, ImageKey: {ImageKey}", 
            tempKey, result.Data);

        return result.Data;
    }

    public async Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return;
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(
                imageKey,
                StorageConstants.ModuleImageBucket);

            if (result.Success)
            {
                _logger.LogInformation("Module image deleted successfully. ImageKey: {ImageKey}", imageKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete module image. ImageKey: {ImageKey}, Message: {Message}", 
                    imageKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting module image. ImageKey: {ImageKey}", imageKey);
        }
    }

    public string BuildImageUrl(string? imageKey)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.ModuleImageBucket, imageKey);
    }
}

