using LearningEnglish.Application.Common;

namespace LearningEnglish.Application.Interface.Infrastructure.ImageService;

public interface IEssayAttachmentService
{
    Task<string> CommitAttachmentAsync(string tempKey, CancellationToken cancellationToken = default);

    Task DeleteAttachmentAsync(string attachmentKey, CancellationToken cancellationToken = default);

    string BuildAttachmentUrl(string? attachmentKey);

    Task<ServiceResponse<Stream>> DownloadAttachmentAsync(string attachmentKey, CancellationToken cancellationToken = default);
}

