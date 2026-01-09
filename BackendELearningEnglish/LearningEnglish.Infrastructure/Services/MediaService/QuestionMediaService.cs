using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class QuestionMediaService : IQuestionMediaService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<QuestionMediaService> _logger;

    public QuestionMediaService(
        IMinioFileStorage minioFileStorage,
        ILogger<QuestionMediaService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public async Task<string> CommitMediaAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.QuestionBucket,
            StorageConstants.QuestionFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError(
                "Failed to commit question media. TempKey: {TempKey}, Message: {Message}",
                tempKey,
                result.Message);

            throw new InvalidOperationException($"Failed to commit question media: {result.Message}");
        }

        _logger.LogInformation(
            "Question media committed successfully. TempKey: {TempKey}, MediaKey: {MediaKey}",
            tempKey,
            result.Data);

        return result.Data;
    }

    public async Task DeleteMediaAsync(string mediaKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(mediaKey))
        {
            return;
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(
                mediaKey,
                StorageConstants.QuestionBucket);

            if (result.Success)
            {
                _logger.LogInformation("Question media deleted successfully. MediaKey: {MediaKey}", mediaKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete question media. MediaKey: {MediaKey}, Message: {Message}",
                    mediaKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting question media. MediaKey: {MediaKey}", mediaKey);
        }
    }

    public string BuildMediaUrl(string? mediaKey)
    {
        if (string.IsNullOrWhiteSpace(mediaKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.QuestionBucket, mediaKey);
    }
}
