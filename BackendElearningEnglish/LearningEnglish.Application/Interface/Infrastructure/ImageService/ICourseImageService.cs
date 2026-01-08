namespace LearningEnglish.Application.Interface.Infrastructure.ImageService;

public interface ICourseImageService
{
    Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default);

    string BuildImageUrl(string? imageKey);
}

