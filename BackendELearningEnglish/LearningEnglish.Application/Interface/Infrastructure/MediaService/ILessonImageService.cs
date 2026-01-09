namespace LearningEnglish.Application.Interface.Infrastructure.MediaService;

public interface ILessonImageService
{
    Task<string> CommitImageAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteImageAsync(string imageKey, CancellationToken cancellationToken = default);

    string BuildImageUrl(string? imageKey);
}

