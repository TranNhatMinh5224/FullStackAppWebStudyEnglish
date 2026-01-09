using LearningEnglish.Application.Common;
using LearningEnglish.Application.Interface;
using LearningEnglish.Application.Interface.Infrastructure.MediaService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace LearningEnglish.Infrastructure.Services.MediaService;

public class AvatarService : IAvatarService
{
    private readonly IMinioFileStorage _minioFileStorage;
    private readonly ILogger<AvatarService> _logger;

    public AvatarService(
        IMinioFileStorage minioFileStorage,
        ILogger<AvatarService> logger)
    {
        _minioFileStorage = minioFileStorage;
        _logger = logger;
    }

    public string BuildAvatarUrl(string? avatarKey)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
        {
            return string.Empty;
        }

        // Handle both cases: with folder prefix and without
        var key = avatarKey.StartsWith($"{StorageConstants.AvatarFolder}/")
            ? avatarKey
            : $"{StorageConstants.AvatarFolder}/{avatarKey}";

        return BuildPublicUrl.BuildURL(StorageConstants.AvatarBucket, key);
    }

    public async Task<ServiceResponse<string>> UploadTempAvatarAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "File is empty or null"
            };
        }

        var result = await _minioFileStorage.UpLoadFileTempAsync(
            file,
            StorageConstants.AvatarBucket,
            StorageConstants.AvatarFolder);

        if (!result.Success || result.Data == null)
        {
            _logger.LogError("Failed to upload temp avatar. Message: {Message}", result.Message);
            return new ServiceResponse<string>
            {
                Success = false,
                Message = $"Failed to upload avatar: {result.Message}"
            };
        }

        _logger.LogInformation("Avatar uploaded to temp. TempKey: {TempKey}", result.Data.TempKey);
        return new ServiceResponse<string>
        {
            Success = true,
            Data = result.Data.TempKey
        };
    }

    public async Task<ServiceResponse<string>> CommitAvatarAsync(string tempKey)
    {
        if (string.IsNullOrWhiteSpace(tempKey))
        {
            return new ServiceResponse<string>
            {
                Success = false,
                Message = "Temp key cannot be empty"
            };
        }

        var result = await _minioFileStorage.CommitFileAsync(
            tempKey,
            StorageConstants.AvatarBucket,
            StorageConstants.AvatarFolder);

        if (!result.Success || string.IsNullOrWhiteSpace(result.Data))
        {
            _logger.LogError(
                "Failed to commit avatar. TempKey: {TempKey}, Message: {Message}",
                tempKey,
                result.Message);

            return new ServiceResponse<string>
            {
                Success = false,
                Message = $"Failed to commit avatar: {result.Message}"
            };
        }

        _logger.LogInformation(
            "Avatar committed successfully. TempKey: {TempKey}, AvatarKey: {AvatarKey}",
            tempKey,
            result.Data);

        return new ServiceResponse<string>
        {
            Success = true,
            Data = result.Data
        };
    }

    public async Task DeleteAvatarAsync(string avatarKey)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
        {
            return;
        }

        try
        {
            await _minioFileStorage.DeleteFileAsync(avatarKey, StorageConstants.AvatarBucket);
            _logger.LogInformation("Avatar deleted successfully. AvatarKey: {AvatarKey}", avatarKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete avatar. AvatarKey: {AvatarKey}", avatarKey);
        }
    }
}

