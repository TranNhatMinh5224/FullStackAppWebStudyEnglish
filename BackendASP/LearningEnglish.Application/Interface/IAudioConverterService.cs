namespace LearningEnglish.Application.Interface
{
    public interface IAudioConverterService
    {
        /// <summary>
        /// Convert any audio format to WAV 16kHz Mono 16-bit
        /// </summary>
        Task<byte[]> ConvertToWavAsync(byte[] inputBytes, string inputFormat);
        
        /// <summary>
        /// Validate and convert WAV to correct format if needed
        /// </summary>
        Task<byte[]> ValidateWavFormatAsync(byte[] wavBytes);
        
        /// <summary>
        /// Detect audio format from file name
        /// </summary>
        string DetectAudioFormat(string fileName);
    }
}
