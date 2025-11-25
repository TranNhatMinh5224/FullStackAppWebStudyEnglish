using LearningEnglish.Application.DTOs;
using LearningEnglish.Application.Interface;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LearningEnglish.Infrastructure.Services
{
    public class AzureSpeechService : IAzureSpeechService
    {
        private readonly string _subscriptionKey;
        private readonly string _region;
        private readonly HttpClient _httpClient;
        private readonly ILogger<AzureSpeechService> _logger;

        public AzureSpeechService(
            IConfiguration configuration, 
            HttpClient httpClient,
            ILogger<AzureSpeechService> logger)
        {
            _subscriptionKey = configuration["AzureSpeech:SubscriptionKey"] ?? 
                throw new ArgumentNullException("AzureSpeech:SubscriptionKey is not configured");
            _region = configuration["AzureSpeech:Region"] ?? 
                throw new ArgumentNullException("AzureSpeech:Region is not configured");
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<AzureSpeechAssessmentResult> AssessPronunciationAsync(
            string audioUrl, 
            string referenceText, 
            string locale = "en-US")
        {
            try
            {
                // Download audio from MinIO URL
                var audioBytes = await _httpClient.GetByteArrayAsync(audioUrl);
                using var audioStream = new MemoryStream(audioBytes);
                
                return await AssessPronunciationFromStreamAsync(audioStream, referenceText, locale);
            }
            catch (Exception ex)
            {
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Failed to download audio: {ex.Message}"
                };
            }
        }

        public async Task<AzureSpeechAssessmentResult> AssessPronunciationFromStreamAsync(
            Stream audioStream, 
            string referenceText, 
            string locale = "en-US")
        {
            try
            {
                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechRecognitionLanguage = locale;

                // Set up audio stream
                byte[] audioBytes;
                if (audioStream is MemoryStream ms)
                {
                    audioBytes = ms.ToArray();
                }
                else
                {
                    using var memStream = new MemoryStream();
                    await audioStream.CopyToAsync(memStream);
                    audioBytes = memStream.ToArray();
                }

                using var audioInputStream = AudioInputStream.CreatePushStream();
                using var audioConfig = AudioConfig.FromStreamInput(audioInputStream);
                using var recognizer = new SpeechRecognizer(config, audioConfig);

                // Configure pronunciation assessment via JSON
                var pronunciationAssessmentJson = $@"{{
                    ""ReferenceText"": ""{referenceText}"",
                    ""GradingSystem"": ""HundredMark"",
                    ""Granularity"": ""Phoneme"",
                    ""EnableMiscue"": true
                }}";
                
                recognizer.Properties.SetProperty("SPEECH-PronunciationAssessment", pronunciationAssessmentJson);

                // Push audio data
                audioInputStream.Write(audioBytes);
                audioInputStream.Close();

                // Recognize speech
                var result = await recognizer.RecognizeOnceAsync();

                if (result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Parse JSON result
                    var jsonResult = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);
                    
                    // Try to extract scores from JSON
                    try
                    {
                        var jsonDoc = System.Text.Json.JsonDocument.Parse(jsonResult);
                        var root = jsonDoc.RootElement;
                        
                        double accuracyScore = 0;
                        double fluencyScore = 0;
                        double completenessScore = 0;
                        double pronScore = 0;
                        
                        if (root.TryGetProperty("NBest", out var nBest) && nBest.GetArrayLength() > 0)
                        {
                            var firstResult = nBest[0];
                            if (firstResult.TryGetProperty("PronunciationAssessment", out var pronAssessment))
                            {
                                if (pronAssessment.TryGetProperty("AccuracyScore", out var acc))
                                    accuracyScore = acc.GetDouble();
                                if (pronAssessment.TryGetProperty("FluencyScore", out var flu))
                                    fluencyScore = flu.GetDouble();
                                if (pronAssessment.TryGetProperty("CompletenessScore", out var comp))
                                    completenessScore = comp.GetDouble();
                                if (pronAssessment.TryGetProperty("PronScore", out var pron))
                                    pronScore = pron.GetDouble();
                            }
                        }
                        
                        return new AzureSpeechAssessmentResult
                        {
                            Success = true,
                            AccuracyScore = accuracyScore,
                            FluencyScore = fluencyScore,
                            CompletenessScore = completenessScore,
                            PronunciationScore = pronScore,
                            RecognizedText = result.Text,
                            DetailedResultJson = jsonResult,
                            RawResponse = jsonResult
                        };
                    }
                    catch (Exception parseEx)
                    {
                        // If parsing fails, return what we have
                        return new AzureSpeechAssessmentResult
                        {
                            Success = true,
                            AccuracyScore = 0,
                            FluencyScore = 0,
                            CompletenessScore = 0,
                            PronunciationScore = 0,
                            RecognizedText = result.Text,
                            DetailedResultJson = jsonResult,
                            RawResponse = jsonResult,
                            ErrorMessage = $"Speech recognized but score parsing failed: {parseEx.Message}"
                        };
                    }
                }
                else if (result.Reason == ResultReason.NoMatch)
                {
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = "No speech could be recognized from the audio."
                    };
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = CancellationDetails.FromResult(result);
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = $"Speech recognition canceled: {cancellation.Reason}. Details: {cancellation.ErrorDetails}"
                    };
                }
                else
                {
                    return new AzureSpeechAssessmentResult
                    {
                        Success = false,
                        ErrorMessage = $"Unexpected result reason: {result.Reason}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new AzureSpeechAssessmentResult
                {
                    Success = false,
                    ErrorMessage = $"Azure Speech Service error: {ex.Message}"
                };
            }
        }

        public async Task<Stream?> GenerateSpeechAsync(
            string text, 
            string locale = "en-US", 
            string voiceName = "en-US-JennyNeural")
        {
            try
            {
                var config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
                config.SpeechSynthesisVoiceName = voiceName;
                config.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio16Khz32KBitRateMonoMp3);

                using var synthesizer = new SpeechSynthesizer(config, null);
                var result = await synthesizer.SpeakTextAsync(text);

                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    _logger.LogInformation("Speech synthesized for text: {Text}", text);
                    var audioData = result.AudioData;
                    return new MemoryStream(audioData);
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    _logger.LogError("Speech synthesis canceled: {Reason}. Details: {ErrorDetails}", 
                        cancellation.Reason, cancellation.ErrorDetails);
                    return null;
                }
                else
                {
                    _logger.LogWarning("Speech synthesis failed with reason: {Reason}", result.Reason);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating speech for text: {Text}", text);
                return null;
            }
        }
    }
}
