namespace LearningEnglish.Application.Interface.Infrastructure.ImageService;

public interface IFlashCardMediaService
{
    Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default);

    Task<string> CommitAudioAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default);

    Task DeleteAudioAsync(string audioKey, CancellationToken cancellationToken = default);

    string BuildImageUrl(string? imageKey);

    string BuildAudioUrl(string? audioKey);
}

