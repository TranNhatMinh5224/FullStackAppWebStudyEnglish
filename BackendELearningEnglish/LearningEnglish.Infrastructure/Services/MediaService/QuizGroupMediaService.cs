using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class QuizGroupMediaService : IQuizGroupMediaService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<QuizGroupMediaService> _logger;

    public QuizGroupMediaService(
        IMinioFileStorage minioFileStorage,
        ILogger<QuizGroupMediaService> logger)
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
            StorageConstants.QuizGroupBucket,
            StorageConstants.QuizGroupFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError(
                "Failed to commit quiz group image. TempKey: {TempKey}, Message: {Message}",
                tempKey,
                result.Message);

            throw new InvalidOperationException($"Failed to commit quiz group image: {result.Message}");
        }

        _logger.LogInformation(
            "Quiz group image committed successfully. TempKey: {TempKey}, ImageKey: {ImageKey}",
            tempKey,
            result.Data);

        return result.Data;
    }

    public async Task<string> CommitVideoAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.QuizGroupBucket,
            StorageConstants.QuizGroupFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError(
                "Failed to commit quiz group video. TempKey: {TempKey}, Message: {Message}",
                tempKey,
                result.Message);

            throw new InvalidOperationException($"Failed to commit quiz group video: {result.Message}");
        }

        _logger.LogInformation(
            "Quiz group video committed successfully. TempKey: {TempKey}, VideoKey: {VideoKey}",
            tempKey,
            result.Data);

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
                StorageConstants.QuizGroupBucket);

            if (result.Success)
            {
                _logger.LogInformation("Quiz group image deleted successfully. ImageKey: {ImageKey}", imageKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete quiz group image. ImageKey: {ImageKey}, Message: {Message}",
                    imageKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting quiz group image. ImageKey: {ImageKey}", imageKey);
        }
    }

    public async Task DeleteVideoAsync(string videoKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(videoKey))
        {
            return;
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(
                videoKey,
                StorageConstants.QuizGroupBucket);

            if (result.Success)
            {
                _logger.LogInformation("Quiz group video deleted successfully. VideoKey: {VideoKey}", videoKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete quiz group video. VideoKey: {VideoKey}, Message: {Message}",
                    videoKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting quiz group video. VideoKey: {VideoKey}", videoKey);
        }
    }

    public string BuildImageUrl(string? imageKey)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.QuizGroupBucket, imageKey);
    }

    public string BuildVideoUrl(string? videoKey)
    {
        if (string.IsNullOrWhiteSpace(videoKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.QuizGroupBucket, videoKey);
    }
}
