using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    public interface IAzureSpeechService
    {
        // Đánh giá phát âm từ audio URL
        Task<AzureSpeechAssessmentResult> AssessPronunciationAsync(
            string audioUrl,
            string referenceText,
            string locale = "en-US");

        // Đánh giá phát âm từ audio stream
        Task<AzureSpeechAssessmentResult> AssessPronunciationFromStreamAsync(
            Stream audioStream,
            string referenceText,
            string locale = "en-US");

        // Tạo giọng nói từ text
        Task<Stream?> GenerateSpeechAsync(
            string text,
            string locale = "en-US",
            string voiceName = "en-US-JennyNeural");
    }
}
