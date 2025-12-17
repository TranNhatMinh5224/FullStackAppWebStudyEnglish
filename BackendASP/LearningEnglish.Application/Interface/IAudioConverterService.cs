namespace LearningEnglish.Application.Interface
{
    public interface IAudioConverterService
    {
        // Chuyển đổi audio sang WAV
        Task<byte[]> ConvertToWavAsync(byte[] inputBytes, string inputFormat);

        // Kiểm tra và chuyển đổi WAV sang định dạng đúng
        Task<byte[]> ValidateWavFormatAsync(byte[] wavBytes);

        // Xác định định dạng audio từ tên file
        string DetectAudioFormat(string fileName);
    }
}
