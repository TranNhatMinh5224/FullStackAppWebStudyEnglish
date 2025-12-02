using LearningEnglish.Application.Interface;
using Microsoft.Extensions.Logging;
using NAudio.Wave;

namespace LearningEnglish.Infrastructure.Services
{
    public class AudioConverterService : IAudioConverterService
    {
        private readonly ILogger<AudioConverterService> _logger;

        public AudioConverterService(ILogger<AudioConverterService> logger)
        {
            _logger = logger;
        }

        public string DetectAudioFormat(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".wav" => "wav",
                ".webm" => "webm",
                ".ogg" => "ogg",
                ".mp3" => "mp3",
                ".m4a" => "m4a",
                _ => "unknown"
            };
        }

        public async Task<byte[]> ConvertToWavAsync(byte[] inputBytes, string inputFormat)
        {
            _logger.LogInformation("Converting {Format} to WAV 16kHz Mono (size: {Size} bytes)", inputFormat, inputBytes.Length);

            try
            {
                // Create temp directory
                var tempDir = Path.Combine(Path.GetTempPath(), "pronunciation_temp");
                Directory.CreateDirectory(tempDir);

                var tempInput = Path.Combine(tempDir, $"input_{Guid.NewGuid()}{GetExtension(inputFormat)}");
                var tempOutput = Path.Combine(tempDir, $"output_{Guid.NewGuid()}.wav");

                try
                {
                    await File.WriteAllBytesAsync(tempInput, inputBytes);

                    // Use NAudio's MediaFoundationReader (supports WebM, MP3, etc on Windows)
                    using (var reader = new MediaFoundationReader(tempInput))
                    {
                        _logger.LogInformation(
                            "Input audio specs: SampleRate={SampleRate}, Channels={Channels}, Duration={Duration}s",
                            reader.WaveFormat.SampleRate,
                            reader.WaveFormat.Channels,
                            reader.TotalTime.TotalSeconds);

                        // Resample to 16kHz Mono 16-bit
                        var outFormat = new WaveFormat(16000, 16, 1);
                        using (var resampler = new MediaFoundationResampler(reader, outFormat))
                        {
                            resampler.ResamplerQuality = 60; // High quality

                            // Write to WAV file
                            WaveFileWriter.CreateWaveFile(tempOutput, resampler);
                        }
                    }

                    // Read converted file
                    var wavBytes = await File.ReadAllBytesAsync(tempOutput);

                    _logger.LogInformation("Conversion successful. Output size: {Size} bytes", wavBytes.Length);

                    return wavBytes;
                }
                finally
                {
                    // Cleanup temp files
                    if (File.Exists(tempInput))
                    {
                        try { File.Delete(tempInput); } catch { }
                    }
                    if (File.Exists(tempOutput))
                    {
                        try { File.Delete(tempOutput); } catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert audio format {Format}", inputFormat);
                throw new Exception($"Audio conversion failed: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ValidateWavFormatAsync(byte[] wavBytes)
        {
            _logger.LogInformation("Validating WAV format (size: {Size} bytes)", wavBytes.Length);

            try
            {
                using (var ms = new MemoryStream(wavBytes))
                using (var reader = new WaveFileReader(ms))
                {
                    var format = reader.WaveFormat;

                    _logger.LogInformation(
                        "WAV specs: SampleRate={SampleRate}, Channels={Channels}, BitsPerSample={Bits}",
                        format.SampleRate, format.Channels, format.BitsPerSample
                    );

                    // Check if already correct format
                    if (format.SampleRate == 16000 &&
                        format.Channels == 1 &&
                        format.BitsPerSample == 16)
                    {
                        _logger.LogInformation("WAV format is already correct");
                        return wavBytes;
                    }

                    // Need to resample
                    _logger.LogInformation("Resampling WAV to 16kHz Mono 16-bit");

                    var tempInput = Path.GetTempFileName();
                    var tempOutput = Path.GetTempFileName() + ".wav";

                    try
                    {
                        await File.WriteAllBytesAsync(tempInput, wavBytes);

                        using (var inputReader = new WaveFileReader(tempInput))
                        {
                            var outFormat = new WaveFormat(16000, 16, 1);
                            using (var resampler = new MediaFoundationResampler(inputReader, outFormat))
                            {
                                resampler.ResamplerQuality = 60;
                                WaveFileWriter.CreateWaveFile(tempOutput, resampler);
                            }
                        }

                        var convertedBytes = await File.ReadAllBytesAsync(tempOutput);
                        _logger.LogInformation("WAV resampling successful. Output size: {Size} bytes", convertedBytes.Length);
                        return convertedBytes;
                    }
                    finally
                    {
                        if (File.Exists(tempInput))
                        {
                            try { File.Delete(tempInput); } catch { }
                        }
                        if (File.Exists(tempOutput))
                        {
                            try { File.Delete(tempOutput); } catch { }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate/convert WAV format");
                throw new Exception($"WAV validation failed: {ex.Message}", ex);
            }
        }

        private string GetExtension(string format)
        {
            return format.ToLowerInvariant() switch
            {
                "webm" => ".webm",
                "ogg" => ".ogg",
                "mp3" => ".mp3",
                "wav" => ".wav",
                "m4a" => ".m4a",
                _ => ".tmp"
            };
        }
    }
}
