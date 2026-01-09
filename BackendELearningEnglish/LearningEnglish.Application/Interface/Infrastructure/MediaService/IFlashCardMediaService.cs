using Microsoft.AspNetCore.Http;

namespace LearningEnglish.Application.Interface.Infrastructure.MediaService;

public interface IFlashCardMediaService
{
    /// <summary>
    /// Upload image vào temp folder
    /// </summary>
    Task<string?> UploadTempImageAsync(IFormFile file, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Upload audio vào temp folder
    /// </summary>
    Task<string?> UploadTempAudioAsync(IFormFile file, CancellationToken cancellationToken = default);
    
    Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default);

    Task<string> CommitAudioAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default);

    Task DeleteAudioAsync(string audioKey, CancellationToken cancellationToken = default);

    string BuildImageUrl(string? imageKey);

    string BuildAudioUrl(string? audioKey);
}

