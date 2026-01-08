using LearningEnglish.Application.Interface.Infrastructure.ImageService;
using LearningEnglish.Infrastructure.Common.Constants;
using LearningEnglish.Infrastructure.Common.Helpers;

namespace LearningEnglish.Infrastructure.Services.ImageService;

public class AvatarService : IAvatarService
{
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
}

