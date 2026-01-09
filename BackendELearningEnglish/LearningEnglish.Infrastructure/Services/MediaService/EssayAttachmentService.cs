using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class EssayAttachmentService : IEssayAttachmentService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<EssayAttachmentService> _logger;

    public EssayAttachmentService(
        IMinioFileStorage minioFileStorage,
        ILogger<EssayAttachmentService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public async Task<string> CommitAttachmentAsync(string tempKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            throw new ArgumentException("Temp key cannot be null or empty", nameof(tempKey));
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.EssayAttachmentBucket,
            StorageConstants.EssayAttachmentFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError("Failed to commit essay attachment. TempKey: {TempKey}, Message: {Message}", 
                tempKey, result.Message);
            throw new InvalidOperationException($"Failed to commit essay attachment: {result.Message}");
        }

        _logger.LogInformation("Essay attachment committed successfully. TempKey: {TempKey}, AttachmentKey: {AttachmentKey}", 
            tempKey, result.Data);

        return result.Data;
    }

    public async Task DeleteAttachmentAsync(string attachmentKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(attachmentKey))
        {
            return;
        }

        try
        {
            var result = await _minioFileStorage.DeleteFileAsync(
                attachmentKey,
                StorageConstants.EssayAttachmentBucket);

            if (result.Success)
            {
                _logger.LogInformation("Essay attachment deleted successfully. AttachmentKey: {AttachmentKey}", attachmentKey);
            }
            else
            {
                _logger.LogWarning("Failed to delete essay attachment. AttachmentKey: {AttachmentKey}, Message: {Message}", 
                    attachmentKey, result.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error deleting essay attachment. AttachmentKey: {AttachmentKey}", attachmentKey);
        }
    }

    public string BuildAttachmentUrl(string? attachmentKey)
    {
        if (string.IsNullOrWhiteSpace(attachmentKey))
        {
            return string.Empty;
        }

        return BuildPublicUrl.BuildURL(StorageConstants.EssayAttachmentBucket, attachmentKey);
    }

    public async Task<ServiceResponse<Stream>> DownloadAttachmentAsync(string attachmentKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(attachmentKey))
        {
            return new ServiceResponse<Stream>
            {
                Success = false,
                StatusCode = 400,
                Message = "Attachment key cannot be empty"
            };
        }

        var result = await _minioFileStorage.DownloadFileAsync(
            attachmentKey,
            StorageConstants.EssayAttachmentBucket);

        return result;
    }
}

