namespace LearningEnglish.Application.Interface.Infrastructure.MediaService;

public interface ILectureMediaService
{
    Task<string> CommitMediaAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteMediaAsync(string mediaKey, CancellationToken cancellationToken = default);

    string BuildMediaUrl(string? mediaKey);
}

