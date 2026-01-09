namespace LearningEnglish.Application.Interface.Infrastructure.MediaService;

public interface IEssayMediaService
{
    Task<string> CommitAudioAsync(string tempKey, CancellationToken cancellationToken = default);

    Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteAudioAsync(string audioKey, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default);

    string BuildAudioUrl(string? audioKey);

    string BuildImageUrl(string? imageKey);
}

