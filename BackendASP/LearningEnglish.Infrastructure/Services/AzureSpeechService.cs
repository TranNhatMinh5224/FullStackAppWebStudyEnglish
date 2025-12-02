using LearningEnglish.Application.Configurations;
using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LearningEnglish.Infrastructure.Services
{
    public class AzureSpeechService : IAzureSpeechService
    {
        private readonly AzureSpeechOptions _options;
        private readonly HttpClient _httpClient;
        private readonly IAudioConverterService _audioConverter;
        private readonly ILogger<AzureSpeechService> _logger;

        public AzureSpeechService(
            IOptions<AzureSpeechOptions> options,
            HttpClient httpClient,
            IAudioConverterService audioConverter,
            ILogger<AzureSpeechService> logger)
        {
            _options = options.Value;
            _httpClient = httpClient;
            _audioConverter = audioConverter;
            _logger = logger;
        }

        public async Task<AzureSpeechAssessmentResult> AssessPronunciationAsync(
            string audioUrl,
            string referenceText,
            string locale = "en-US")
        {
            try
            {
                _logger.LogInformation("Starting pronunciation assessment for text: '{Text}'", referenceText);

                // 1. Download audio from MinIO
                _logger.LogInformation("Downloading audio from {Url}", audioUrl);
                var audioBytes = await _httpClient.GetByteArrayAsync(audioUrl);
                _logger.LogInformation("Downloaded {Size} bytes", audioBytes.Length);

                // 2. Detect format
                var format = _audioConverter.DetectAudioFormat(audioUrl);
                _logger.LogInformation("Detected audio format: {Format}", format);

                // 3. Convert to WAV 16kHz Mono
                byte[] wavBytes;
                if (format == "wav")
                {
                    // Validate and convert if needed
                    wavBytes = await _audioConverter.ValidateWavFormatAsync(audioBytes);
                }
                else
                {
                    // Convert from other formats
                    wavBytes = await _audioConverter.ConvertToWavAsync(audioBytes, format);
                }

                // 4. Create temp WAV file for Azure SDK
                var tempWavFile = Path.Combine(Path.GetTempPath(), $"assessment_{Guid.NewGuid()}.wav");
                await File.WriteAllBytesAsync(tempWavFile, wavBytes);
                _logger.LogInformation("Created temp WAV file: {Path}", tempWavFile);

                try
                {
                    // 5. Setup Azure Speech SDK
                    var speechConfig = SpeechConfig.FromSubscription(_options.Key, _options.Region);
                    speechConfig.SpeechRecognitionLanguage = locale;
                    speechConfig.SetProperty(
                        PropertyId.SpeechServiceResponse_RequestDetailedResultTrueFalse,
                        "true"
                    );

                    // 6. Configure pronunciation assessment
                    var pronunciationConfig = new PronunciationAssessmentConfig(
                        referenceText,
                        GradingSystem.HundredMark,
                        Granularity.Phoneme,
                        enableMiscue: true
                    );

                    // 7. Create audio and recognizer
                    using var audioConfig = AudioConfig.FromWavFileInput(tempWavFile);
                    using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

                    pronunciationConfig.ApplyTo(recognizer);

                    // 8. Recognize with timeout
                    _logger.LogInformation("Calling Azure Speech Recognition API...");
                    var cancellationTokenSource = new CancellationTokenSource(
                        TimeSpan.FromSeconds(_options.TimeoutSeconds)
                    );

                    var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);
                    _logger.LogInformation("Recognition completed with reason: {Reason}", result.Reason);

                    // 9. Parse and return results
                    return ParseAzureResult(result, referenceText);
                }
                finally
                {
                    // Cleanup temp WAV file
                    if (File.Exists(tempWavFile))
                    {
                        try
                        {
                            File.Delete(tempWavFile);
                            _logger.LogInformation("Cleaned up temp WAV file");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temp WAV file: {Path}", tempWavFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pronunciation assessment failed");
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Assessment failed: {ex.Message}"
                };
            }
        }

        public async Task<AzureSpeechAssessmentResult> AssessPronunciationFromStreamAsync(
            Stream audioStream,
            string referenceText,
            string locale = "en-US")
        {
            // Not implemented yet - can add if needed
            throw new NotImplementedException("Stream-based assessment not implemented yet");
        }

        public async Task<Stream?> GenerateSpeechAsync(
            string text,
            string locale = "en-US",
            string voiceName = "en-US-JennyNeural")
        {
            // Not implemented yet - can add for TTS feature
            throw new NotImplementedException("Text-to-speech not implemented yet");
        }

        private AzureSpeechAssessmentResult ParseAzureResult(
            SpeechRecognitionResult result,
            string referenceText)
        {
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                try
                {
                    // Get detailed JSON result
                    var jsonResult = result.Properties.GetProperty(
                        PropertyId.SpeechServiceResponse_JsonResult
                    );

                    _logger.LogDebug("Azure JSON Response: {Json}", jsonResult);

                    var jsonDoc = JsonDocument.Parse(jsonResult);
                    var nBest = jsonDoc.RootElement.GetProperty("NBest")[0];

                    // Extract overall scores
                    var pronunciationAssessment = nBest.GetProperty("PronunciationAssessment");
                    var accuracyScore = pronunciationAssessment.GetProperty("AccuracyScore").GetDouble();
                    var fluencyScore = pronunciationAssessment.GetProperty("FluencyScore").GetDouble();
                    var completenessScore = pronunciationAssessment.GetProperty("CompletenessScore").GetDouble();
                    var pronunciationScore = pronunciationAssessment.GetProperty("PronScore").GetDouble();

                    _logger.LogInformation(
                        "Scores - Accuracy: {Accuracy}, Fluency: {Fluency}, Completeness: {Completeness}, Pronunciation: {Pronunciation}",
                        accuracyScore, fluencyScore, completenessScore, pronunciationScore
                    );

                    // Extract word-level details
                    var words = new List<WordPronunciationDetail>();
                    var problemPhonemes = new List<string>();
                    var strongPhonemes = new List<string>();

                    if (nBest.TryGetProperty("Words", out var wordsArray))
                    {
                        foreach (var word in wordsArray.EnumerateArray())
                        {
                            var wordDetail = new WordPronunciationDetail
                            {
                                Word = word.GetProperty("Word").GetString() ?? "",
                                AccuracyScore = word.GetProperty("PronunciationAssessment")
                                    .GetProperty("AccuracyScore").GetDouble(),
                                ErrorType = word.GetProperty("PronunciationAssessment")
                                    .GetProperty("ErrorType").GetString() ?? "None"
                            };

                            if (word.TryGetProperty("Offset", out var offset))
                                wordDetail.Offset = offset.GetInt32();
                            if (word.TryGetProperty("Duration", out var duration))
                                wordDetail.Duration = duration.GetInt32();

                            // Parse phonemes
                            if (word.TryGetProperty("Phonemes", out var phonemesArray))
                            {
                                foreach (var phoneme in phonemesArray.EnumerateArray())
                                {
                                    var phonemeIPA = phoneme.GetProperty("Phoneme").GetString() ?? "";
                                    var phonemeScore = phoneme.GetProperty("PronunciationAssessment")
                                        .GetProperty("AccuracyScore").GetDouble();

                                    var phonemeDetail = new PhonemeDetail
                                    {
                                        Phoneme = phonemeIPA,
                                        PhonemeDisplay = ConvertIPAToDisplay(phonemeIPA),
                                        AccuracyScore = phonemeScore
                                    };

                                    if (phoneme.TryGetProperty("Offset", out var pOffset))
                                        phonemeDetail.Offset = pOffset.GetInt32();
                                    if (phoneme.TryGetProperty("Duration", out var pDuration))
                                        phonemeDetail.Duration = pDuration.GetInt32();

                                    // Classify phonemes
                                    if (phonemeScore < 60)
                                        problemPhonemes.Add(phonemeDetail.PhonemeDisplay);
                                    else if (phonemeScore >= 85)
                                        strongPhonemes.Add(phonemeDetail.PhonemeDisplay);

                                    wordDetail.Phonemes.Add(phonemeDetail);
                                }
                            }

                            words.Add(wordDetail);
                        }
                    }

                    return new AzureSpeechAssessmentResult
                    {
                        Success = true,
                        AccuracyScore = accuracyScore,
                        FluencyScore = fluencyScore,
                        CompletenessScore = completenessScore,
                        PronunciationScore = pronunciationScore,
                        RecognizedText = result.Text,
                        Words = words,
                        ProblemPhonemes = problemPhonemes.Distinct().ToList(),
                        StrongPhonemes = strongPhonemes.Distinct().ToList(),
                        DetailedResultJson = jsonResult,
                        RawResponse = jsonResult
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse Azure response");
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to parse Azure response: {ex.Message}"
                    };
                }
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                _logger.LogWarning("No speech recognized");
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = "Could not recognize speech. Please speak more clearly or check audio quality."
                };
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                _logger.LogError("Recognition canceled: {Reason}, ErrorCode: {Code}, Details: {Details}",
                    cancellation.Reason, cancellation.ErrorCode, cancellation.ErrorDetails);

                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Recognition canceled: {cancellation.ErrorDetails}"
                };
            }
            else
            {
                _logger.LogWarning("Unexpected recognition result: {Reason}", result.Reason);
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Unexpected result: {result.Reason}"
                };
            }
        }

        private string ConvertIPAToDisplay(string ipa)
        {
            // Map IPA symbols to user-friendly display
            var mapping = new Dictionary<string, string>
            {
                { "θ", "th" },
                { "ð", "th" },
                { "ʃ", "sh" },
                { "ʒ", "zh" },
                { "ʧ", "ch" },
                { "ʤ", "j" },
                { "ŋ", "ng" },
                { "ɜr", "er" },
                { "ɜ", "er" },
                { "ə", "uh" },
                { "ɪ", "i" },
                { "i", "ee" },
                { "ɑ", "ah" },
                { "æ", "a" },
                { "ʊ", "oo" },
                { "u", "oo" },
                { "oʊ", "oh" },
                { "aʊ", "ow" },
                { "aɪ", "ay" }
            };

            return mapping.TryGetValue(ipa, out var display) ? display : ipa;
        }
    }
}
