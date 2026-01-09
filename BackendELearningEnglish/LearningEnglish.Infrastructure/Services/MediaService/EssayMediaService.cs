using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class EssayMediaService : IEssayMediaService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<EssayMediaService> _logger;

    public EssayMediaService(
        IMinioFileStorage minioFileStorage,
        ILogger<EssayMediaService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public async Task<string> CommitAudioAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.EssayAudioBucket,
            StorageConstants.EssayAudioFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit essay audio. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit essay audio: {result.Message}");
        }

        _logger.LogInformation("Essay audio committed successfully. TempKey: {TempKey}, AudioKey: {AudioKey}", 
            tempKey, result.Data);

        return result.Data;
    }

    public async Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
                StorageConstants.EssayImageBucket,
                StorageConstants.EssayImageFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit essay image. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit essay image: {result.Message}");
        }

        _logger.LogInformation("Essay image committed successfully. TempKey: {TempKey}, ImageKey: {ImageKey}", 
            tempKey, result.Data);

        return result.Data;
    }

    public async Task DeleteAudioAsync(string audioKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(audioKey))
        {
            return;
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(
                audioKey,
                StorageConstants.EssayAudioBucket);

            if (result.Success)
            {
                _logger.LogInformation("Essay audio deleted successfully. AudioKey: {AudioKey}", audioKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete essay audio. AudioKey: {AudioKey}, Message: {Message}", 
                    audioKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting essay audio. AudioKey: {AudioKey}", audioKey);
        }
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
                StorageConstants.EssayImageBucket);

            if (result.Success)
            {
                _logger.LogInformation("Essay image deleted successfully. ImageKey: {ImageKey}", imageKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete essay image. ImageKey: {ImageKey}, Message: {Message}", 
                    imageKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting essay image. ImageKey: {ImageKey}", imageKey);
        }
    }

    public string BuildAudioUrl(string? audioKey)
    {
        if (string.IsNullOrWhiteSpace(audioKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.EssayAudioBucket, audioKey);
    }

    public string BuildImageUrl(string? imageKey)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.EssayImageBucket, imageKey);
    }
}

