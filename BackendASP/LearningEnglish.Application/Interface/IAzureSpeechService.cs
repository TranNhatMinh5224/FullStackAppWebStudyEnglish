using LearningEnglish.Application.DTOs;

namespace LearningEnglish.Application.Interface
{
    /// <summary>
    /// Interface for Azure Speech Service pronunciation assessment and text-to-speech
    /// </summary>
    public interface IAzureSpeechService
    {
        /// <summary>
        /// Assess pronunciation from audio URL
        /// </summary>
        /// <param name="audioUrl">Public URL of the audio file in MinIO</param>
        /// <param name="referenceText">Text that should be read</param>
        /// <param name="locale">Language locale (default: en-US)</param>
        /// <returns>Assessment result with scores</returns>
        Task<AzureSpeechAssessmentResult> AssessPronunciationAsync(
            string audioUrl, 
            string referenceText, 
            string locale = "en-US");

        /// <summary>
        /// Assess pronunciation from audio stream
        /// </summary>
        Task<AzureSpeechAssessmentResult> AssessPronunciationFromStreamAsync(
            Stream audioStream, 
            string referenceText, 
            string locale = "en-US");

        /// <summary>
        /// Generate speech audio from text using Azure Text-to-Speech
        /// </summary>
        /// <param name="text">Text to convert to speech</param>
        /// <param name="locale">Language locale (default: en-US)</param>
        /// <param name="voiceName">Azure voice name (default: en-US-JennyNeural)</param>
        /// <returns>Audio stream in MP3 format</returns>
        Task<Stream?> GenerateSpeechAsync(
            string text, 
            string locale = "en-US", 
            string voiceName = "en-US-JennyNeural");
    }
}
