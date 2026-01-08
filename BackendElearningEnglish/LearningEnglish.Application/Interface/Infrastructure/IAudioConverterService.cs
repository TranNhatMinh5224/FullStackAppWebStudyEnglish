namespace LearningEnglish.Application.Interface
{
    public interface IAudioConverterService
    {
        // Chuyển đổi sang WAV
        Task<byte[]> ConvertToWavAsync(byte[] inputBytes, string inputFormat);

        // Xác thực định dạng WAV
        Task<byte[]> ValidateWavFormatAsync(byte[] wavBytes);

        // Phát hiện định dạng audio
        string DetectAudioFormat(string fileName);
    }
}
