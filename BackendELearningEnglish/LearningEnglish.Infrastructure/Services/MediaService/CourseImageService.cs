using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class CourseImageService : ICourseImageService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<CourseImageService> _logger;

    public CourseImageService(
        IMinioFileStorage minioFileStorage,
        ILogger<CourseImageService> logger)
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
            StorageConstants.CourseImageBucket,
            StorageConstants.CourseImageFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit course image. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit course image: {result.Message}");
        }

        _logger.LogInformation("Course image committed successfully. TempKey: {TempKey}, ImageKey: {ImageKey}", 
            tempKey, result.Data);

        return result.Data;
    }

    public async Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return; // Nothing to delete
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(
                imageKey,
                StorageConstants.CourseImageBucket);

            if (result.Success)
            {
                _logger.LogInformation("Course image deleted successfully. ImageKey: {ImageKey}", imageKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete course image. ImageKey: {ImageKey}, Message: {Message}", 
                    imageKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting course image. ImageKey: {ImageKey}", imageKey);
        }
    }

    public string BuildImageUrl(string? imageKey)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.CourseImageBucket, imageKey);
    }
}

