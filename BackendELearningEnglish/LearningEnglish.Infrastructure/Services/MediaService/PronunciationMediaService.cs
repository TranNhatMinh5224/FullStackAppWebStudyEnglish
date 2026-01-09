using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

/// <summary>
/// Service for handling pronunciation audio files
/// </summary>
public class PronunciationMediaService : IPronunciationMediaService
{
    
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<PronunciationMediaService> _logger;

    public PronunciationMediaService(
        IMinioFileStorage minioFileStorage,
        ILogger<PronunciationMediaService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public string BuildAudioUrl(string? audioKey)
    {
        if (string.IsNullOrWhiteSpace(audioKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.PronunciationBucket, audioKey);
    }

    public async Task DeleteTempAudioAsync(string audioKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(audioKey))
        {
            return;
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(audioKey, StorageConstants.PronunciationBucket);
            if (result.Success)
            {
                _logger.LogInformation("Pronunciation temp audio deleted. Key: {AudioKey}", audioKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete pronunciation temp audio. Key: {AudioKey}, Message: {Message}",
                    audioKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting pronunciation temp audio. Key: {AudioKey}", audioKey);
        }
    }
}
