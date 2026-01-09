using LearningEnglish.Application.Common;
using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Interface.Infrastructure.MediaService;

public interface IAvatarService
{
    /// <summary>
    /// Build URL công khai cho avatar
    /// </summary>
    string BuildAvatarUrl(string? avatarKey);
    
    /// <summary>
    /// Upload avatar vào temp folder (để preview)
    /// </summary>
    /// <param name="file">File avatar cần upload</param>
    /// <returns>ServiceResponse với TempKey</returns>
    Task<ServiceResponse<string>> UploadTempAvatarAsync(IFormFile file);
    
    /// <summary>
    /// Commit avatar từ temp folder sang real folder
    /// </summary>
    /// <param name="tempKey">Key của file trong temp folder</param>
    /// <returns>ServiceResponse với key mới trong real folder</returns>
    Task<ServiceResponse<string>> CommitAvatarAsync(string tempKey);
    
    /// <summary>
    /// Xóa avatar từ storage
    /// </summary>
    /// <param name="avatarKey">Key của file cần xóa</param>
    Task DeleteAvatarAsync(string avatarKey);
}

