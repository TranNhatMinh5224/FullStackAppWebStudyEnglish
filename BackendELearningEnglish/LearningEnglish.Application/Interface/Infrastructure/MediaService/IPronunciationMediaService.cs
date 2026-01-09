namespace LearningEnglish.Application.Interface.Infrastructure.MediaService;

/// <summary>
/// Service for handling pronunciation audio files
/// Used for Azure Speech Service pronunciation assessment
/// </summary>
public interface IPronunciationMediaService
{
    /// <summary>
    /// Build public URL for pronunciation audio (temp files)
    /// </summary>
    /// <param name="audioKey">Key của file audio</param>
    /// <returns>Public URL để Azure Speech Service có thể access</returns>
    string BuildAudioUrl(string? audioKey);

    /// <summary>
    /// Delete temp audio file after processing
    /// </summary>
    /// <param name="audioKey">Key của file audio cần xóa</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteTempAudioAsync(string audioKey, CancellationToken cancellationToken = default);
}
