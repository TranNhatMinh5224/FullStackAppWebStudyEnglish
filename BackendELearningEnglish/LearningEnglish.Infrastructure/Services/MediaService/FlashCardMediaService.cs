using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class FlashCardMediaService : IFlashCardMediaService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<FlashCardMediaService> _logger;

    private const string TempFolder = "temp";

    public FlashCardMediaService(
        IMinioFileStorage minioFileStorage,
        ILogger<FlashCardMediaService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public async Task<string?> UploadTempImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var result = await _minioFileStorage.UpLoadFileTempAsync(file, StorageConstants.FlashCardBucket, TempFolder);
        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("FlashCard image uploaded to temp. TempKey: {TempKey}", result.Data.TempKey);
            return result.Data.TempKey;
        }

        _logger.LogWarning("Failed to upload flashcard image to temp. Message: {Message}", result.Message);
        return null;
    }

    public async Task<string?> UploadTempAudioAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var result = await _minioFileStorage.UpLoadFileTempAsync(file, StorageConstants.FlashCardAudioBucket, TempFolder);
        if (result.Success && result.Data != null)
        {
            _logger.LogInformation("FlashCard audio uploaded to temp. TempKey: {TempKey}", result.Data.TempKey);
            return result.Data.TempKey;
        }

        _logger.LogWarning("Failed to upload flashcard audio to temp. Message: {Message}", result.Message);
        return null;
    }

    public async Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.FlashCardBucket,
            StorageConstants.FlashCardFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit flashcard image. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit flashcard image: {result.Message}");
        }

        _logger.LogInformation("FlashCard image committed successfully. TempKey: {TempKey}, ImageKey: {ImageKey}", 
            tempKey, result.Data);

        return result.Data;
    }

    public async Task<string> CommitAudioAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.FlashCardAudioBucket,
            StorageConstants.FlashCardFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit flashcard audio. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit flashcard audio: {result.Message}");
        }

        _logger.LogInformation("FlashCard audio committed successfully. TempKey: {TempKey}, AudioKey: {AudioKey}", 
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
                StorageConstants.FlashCardBucket);

            if (result.Success)
            {
                _logger.LogInformation("FlashCard image deleted successfully. ImageKey: {ImageKey}", imageKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete flashcard image. ImageKey: {ImageKey}, Message: {Message}", 
                    imageKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting flashcard image. ImageKey: {ImageKey}", imageKey);
        }
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
                StorageConstants.FlashCardAudioBucket);

            if (result.Success)
            {
                _logger.LogInformation("FlashCard audio deleted successfully. AudioKey: {AudioKey}", audioKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete flashcard audio. AudioKey: {AudioKey}, Message: {Message}", 
                    audioKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting flashcard audio. AudioKey: {AudioKey}", audioKey);
        }
    }

    public string BuildImageUrl(string? imageKey)
    {
        if (string.IsNullOrWhiteSpace(imageKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.FlashCardBucket, imageKey);
    }

    public string BuildAudioUrl(string? audioKey)
    {
        if (string.IsNullOrWhiteSpace(audioKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.FlashCardAudioBucket, audioKey);
    }
}

